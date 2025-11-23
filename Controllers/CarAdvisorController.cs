using Microsoft.AspNetCore.Mvc;
using CarAdvisorAPI.Models;
using CarAdvisorAPI.Services;
using CarAdvisorAPI.Data;

namespace CarAdvisorAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarAdvisorController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public CarAdvisorController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<AnalysisResult>> Analyze([FromBody] UserRequest request)
        {
            try
            {
                var userMinBudget = long.Parse(request.MinBudget);
                var userMaxBudget = long.Parse(request.MaxBudget);

                var allAgents = new[]
                {
                    (Name: "Ultra-Luxury", Icon: "💎", Brands: "Porsche, Maserati, Range Rover, Bentley, Lamborghini, Ferrari, Rolls-Royce", Focus: "ultra-luxury segment"),
                    (Name: "Premium", Icon: "⭐", Brands: "BMW, Mercedes-Benz, Audi, Lexus, Volvo, Genesis, Jaguar, Alfa Romeo", Focus: "premium brands"),
                    (Name: "Upper-Mainstream", Icon: "🎯", Brands: "Volkswagen, Mazda, Subaru, Peugeot, Skoda, SEAT, Cupra, Volvo", Focus: "upper-mainstream segment"),
                    (Name: "Mainstream", Icon: "🔧", Brands: "Toyota, Honda, Hyundai, Kia, Nissan, Ford, Mazda, Suzuki", Focus: "reliable mainstream brands"),
                    (Name: "Budget", Icon: "💸", Brands: "Dacia, Fiat, Renault, Opel, Citroën, Vauxhall, Mitsubishi", Focus: "budget-friendly segment"),
                    (Name: "Electric", Icon: "⚡", Brands: "Tesla, BYD, MG, Polestar, Togg, Rivian, Lucid", Focus: "electric and hybrid vehicles")
                };

                // Get market data
                var marketData = GlobalCarMarket.GetMarketData(request.Country);

                // For each agent: can it find relevant cars in the budget range?
                var relevantAgents = new List<(string Name, string Icon, string Brands, string Focus, string Reason)>();

                foreach (var agent in allAgents)
                {
                    var agentBrands = agent.Brands.Split(',').Select(b => b.Trim()).ToList();

                    // Does the car brand of the agent exists in market?
                    var availableFromAgent = marketData.AvailableModels
                        .Where(kvp => agentBrands.Any(ab => kvp.Key.Contains(ab, StringComparison.OrdinalIgnoreCase)))
                        .Any();

                    if (!availableFromAgent)
                    {
                        continue;
                    }

                    // Check segment price range
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
                    _geminiService.CallAgent(a.Name, a.Icon, a.Brands, a.Focus, request, a.Reason)
                ).ToArray();

                var recommendations = await Task.WhenAll(tasks);

                var validRecommendations = recommendations
                    .Where(r => !r.Recommendation.Contains("outside this budget range", StringComparison.OrdinalIgnoreCase) &&
                                !r.Recommendation.Contains("below your budget range", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (validRecommendations.Count == 0)
                {
                    return BadRequest(new { error = "No suitable cars found in your budget range" });
                }

                var winner = await _geminiService.JuryDecision(validRecommendations, request);

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
    }
}