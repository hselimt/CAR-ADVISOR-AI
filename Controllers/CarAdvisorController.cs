using Microsoft.AspNetCore.Mvc;
using CarAdvisorAPI.Models;
using CarAdvisorAPI.Services;
using CarAdvisorAPI.Data;
using System.Text.RegularExpressions;

namespace CarAdvisorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarAdvisorController : ControllerBase
    {
        private readonly IGeminiService _aiService;
        
        // Turkish market price adjustment multiplier
        private const double TurkishInflationRate = 1.25;

        public CarAdvisorController(IGeminiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<AnalysisResult>> Analyze([FromBody] UserRequest request)
        {
            try
            {
                var userMinBudget = long.Parse(request.MinBudget);
                var userMaxBudget = long.Parse(request.MaxBudget);

                // For Turkey: divide budget by divisor before sending to AI
                var aiMinBudget = request.Country == "Turkey" 
                    ? (long)(userMinBudget / TurkishInflationRate) 
                    : userMinBudget;
                var aiMaxBudget = request.Country == "Turkey" 
                    ? (long)(userMaxBudget / TurkishInflationRate) 
                    : userMaxBudget;

                // Create adjusted request for AI
                var aiRequest = new UserRequest
                {
                    MinBudget = aiMinBudget.ToString(),
                    MaxBudget = aiMaxBudget.ToString(),
                    Preferences = request.Preferences,
                    Country = request.Country,
                    Currency = request.Currency
                };

                var allAgents = new[]
                {
                    (Name: "Ultra-Luxury", Icon: "💎", Brands: "Porsche, Maserati, Range Rover, Bentley, Lamborghini, Ferrari, Rolls-Royce", Focus: "ultra-luxury segment"),
                    (Name: "Premium", Icon: "⭐", Brands: "BMW, Mercedes-Benz, Audi, Lexus, Volvo, Genesis, Jaguar, Alfa Romeo", Focus: "premium brands"),
                    (Name: "Upper-Mainstream", Icon: "🎯", Brands: "Volkswagen, Mazda, Subaru, Peugeot, Skoda, SEAT, Cupra, Volvo", Focus: "upper-mainstream segment"),
                    (Name: "Mainstream", Icon: "🔧", Brands: "Toyota, Honda, Hyundai, Kia, Nissan, Ford, Mazda, Suzuki", Focus: "reliable mainstream brands"),
                    (Name: "Budget", Icon: "💸", Brands: "Dacia, Fiat, Renault, Opel, Citroën, Vauxhall, Mitsubishi", Focus: "budget-friendly segment"),
                    (Name: "Electric", Icon: "⚡", Brands: "Tesla, BYD, MG, Polestar, Togg, Rivian, Lucid", Focus: "electric and hybrid vehicles")
                };

                var marketData = GlobalCarMarket.GetMarketData(request.Country);

                var relevantAgents = new List<(string Name, string Icon, string Brands, string Focus, string Reason)>();

                foreach (var agent in allAgents)
                {
                    var agentBrands = agent.Brands.Split(',').Select(b => b.Trim()).ToList();

                    var availableFromAgent = marketData.AvailableModels
                        .Where(kvp => agentBrands.Any(ab => kvp.Key.Contains(ab, StringComparison.OrdinalIgnoreCase)))
                        .Any();

                    if (!availableFromAgent)
                    {
                        continue;
                    }

                    if (marketData.SegmentPriceRanges.TryGetValue(agent.Name, out var segmentRange))
                    {
                        bool hasOverlap = !(segmentRange.Max < userMinBudget || segmentRange.Min > userMaxBudget);

                        if (hasOverlap)
                        {
                            relevantAgents.Add((agent.Name, agent.Icon, agent.Brands, agent.Focus, "in-budget"));
                        }
                        else
                        {
                            if (segmentRange.Min > userMaxBudget)
                            {
                                relevantAgents.Add((agent.Name, agent.Icon, agent.Brands, agent.Focus, "too-expensive"));
                            }
                            else if (segmentRange.Max < userMinBudget)
                            {
                                relevantAgents.Add((agent.Name, agent.Icon, agent.Brands, agent.Focus, "too-cheap"));
                            }
                        }
                    }
                    else
                    {
                        relevantAgents.Add((agent.Name, agent.Icon, agent.Brands, agent.Focus, "in-budget"));
                    }
                }

                var inBudgetAgents = relevantAgents.Where(a => a.Reason == "in-budget").ToList();
                var outOfBudgetAgents = relevantAgents.Where(a => a.Reason != "in-budget").ToList();

                var finalAgents = new List<(string Name, string Icon, string Brands, string Focus, string Reason)>();
                finalAgents.AddRange(inBudgetAgents);

                if (finalAgents.Count < 4)
                {
                    finalAgents.AddRange(outOfBudgetAgents.Take(4 - finalAgents.Count));
                }

                if (finalAgents.Count < 3)
                {
                    return BadRequest(new { error = $"Not enough car segments available for budget {request.MinBudget}-{request.MaxBudget} {request.Currency} in {request.Country}" });
                }

                var tasks = finalAgents.Select(a =>
                    _aiService.CallAgent(a.Name, a.Icon, a.Brands, a.Focus, aiRequest, a.Reason)
                ).ToArray();

                var recommendations = await Task.WhenAll(tasks);

                // Multiply prices by inflation rate
                if (request.Country == "Turkey")
                {
                    foreach (var rec in recommendations)
                    {
                        foreach (var car in rec.Suggestions)
                        {
                            car.Price = AdjustPrice(car.Price, TurkishInflationRate);
                        }
                    }
                }

                var validRecommendations = recommendations
                    .Where(r => !r.Recommendation.Contains("outside this budget range", StringComparison.OrdinalIgnoreCase) &&
                                !r.Recommendation.Contains("below your budget range", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (validRecommendations.Count == 0)
                {
                    return BadRequest(new { error = "No suitable cars found in your budget range" });
                }

                var winner = await _aiService.JuryDecision(validRecommendations, aiRequest);
                
                // Multiply winner price by inflation rate
                if (request.Country == "Turkey" && winner != null)
                {
                    winner.WinningCarPrice = AdjustPrice(winner.WinningCarPrice, TurkishInflationRate);
                }

                return Ok(new AnalysisResult
                {
                    Recommendations = recommendations.ToList(),
                    Winner = winner
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string AdjustPrice(string priceStr, double multiplier)
        {
            if (string.IsNullOrEmpty(priceStr)) return priceStr;

            // Extract numbers from price string (e.g., "1,500,000 TRY" -> 1500000)
            var numbersOnly = Regex.Replace(priceStr, @"[^\d]", "");
            
            if (long.TryParse(numbersOnly, out long price))
            {
                var adjustedPrice = (long)(price * multiplier);
                
                // Round to nearest 100,000
                adjustedPrice = (long)(Math.Round(adjustedPrice / 100000.0) * 100000);
                
                // Format with commas and add ~ prefix
                var formattedPrice = adjustedPrice.ToString("N0");
                
                // Try to preserve original currency with ~ prefix
                if (priceStr.Contains("TRY") || priceStr.Contains("₺"))
                    return $"~{formattedPrice} TRY";
                if (priceStr.Contains("USD") || priceStr.Contains("$"))
                    return $"~${formattedPrice}";
                if (priceStr.Contains("EUR") || priceStr.Contains("€"))
                    return $"~€{formattedPrice}";
                    
                return $"~{formattedPrice}";
            }

            return priceStr;
        }

        private long ExtractPriceNumber(string priceStr)
        {
            if (string.IsNullOrEmpty(priceStr)) return 0;
            var numbersOnly = Regex.Replace(priceStr, @"[^\d]", "");
            return long.TryParse(numbersOnly, out long price) ? price : 0;
        }

    [HttpPost("generate-image")]
        public async Task<ActionResult> GenerateImage([FromBody] ImageRequest request)
        {
            try
            {
                var imageData = await _aiService.GenerateCarImage(request.CarName);
                if (imageData == null)
                {
                    return BadRequest(new { error = "Failed to generate image" });
                }
                return Ok(new { image = imageData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ImageRequest
    {
        public string CarName { get; set; } = string.Empty;
    }
}
