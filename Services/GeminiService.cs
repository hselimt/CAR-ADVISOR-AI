using CarAdvisorAPI.Data;
using CarAdvisorAPI.Models;
using Newtonsoft.Json;
using System.Text;

namespace CarAdvisorAPI.Services
{
    public interface IGeminiService
    {
        Task<AgentRecommendation> CallAgent(string agentName, string agentIcon, string brands, string focus, UserRequest request, string budgetStatus);
        Task<JuryDecision> JuryDecision(List<AgentRecommendation> recommendations, UserRequest request);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // If API Key is missing, use empty string to prevent crash
            _apiKey = configuration["GeminiApiKey"] ?? "";
        }

        public async Task<AgentRecommendation> CallAgent(string agentName, string agentIcon, string brands, string focus, UserRequest request, string budgetStatus)
        {
            var currentDate = DateTime.Now.ToString("MMMM yyyy");
            var currentYear = DateTime.Now.Year;
            var minCarYear = currentYear - 8;

            // Logic to handle budget warnings inside the prompt context
            string budgetContext = budgetStatus switch
            {
                "too-expensive" => "⚠️ CRITICAL: Your segment is ABOVE the client's budget. You must acknowledge this but recommend the closest options or used alternatives.",
                "too-cheap" => "⚠️ CRITICAL: Your segment is BELOW the client's budget. You must acknowledge the client can afford more premium cars.",
                _ => "✅ Your segment fits the budget."
            };

            var prompt = $@"
                ROLE: You are a professional automotive expert specializing in {focus} vehicles for the {request.Country} market.
                
                CONTEXT:
                - Current Date: {currentDate}
                - Target Market: {request.Country}
                - Client Budget: {request.MinBudget} - {request.MaxBudget} {request.Currency}
                - Client Preferences: {request.Preferences}
                - Your Specific Brands: {brands}
                - Budget Status: {budgetContext}

                TASK: Recommend 2-3 specific vehicles ({minCarYear}-{currentYear} models) currently available in {request.Country}.

                ⚠️ CRITICAL REQUIREMENTS (Apply these strictly):
                1. USE REAL MARKET PRICES: Prices must reflect {currentDate} showroom/market reality in {request.Country}.
                2. DETAILED SPECS: Do not just say ""Fast"". Give HP, Torque, and Seconds.
                3. SAFETY FIRST: You must mention specific safety features and NCAP ratings.
                4. OWNERSHIP REALITY: Mention fuel consumption and maintenance explicitly.

                OUTPUT FORMAT:
                Return ONLY raw JSON. Do not include markdown formatting like ```json.
                Map your detailed analysis into these specific JSON fields:

                {{
                    ""suggestions"": [
                        {{
                            ""make"": ""Brand Name"",
                            ""model"": ""Model Name"",
                            ""year"": ""Year (e.g. 2023)"",
                            ""price"": ""Exact Price with Currency (e.g. 1,500,000 {request.Currency})"",
                            
                            ""engine"": ""Combine: Engine Type, Capacity (cc), Horsepower (HP), Torque (Nm), 0-100 km/h (sec), Transmission (Type/Gears), Drive (FWD/RWD/AWD)"",
                            
                            ""fuel"": ""Combine: Fuel Type, Tank Size, Consumption (City/Hwy/Combined L/100km), Est. Monthly Cost"",
                            
                            ""safetyRating"": ""Combine: Euro NCAP Stars, Airbag Count, Active Safety Tech (ABS, ESP, Lane Assist, Blind Spot, etc.)"",
                            
                            ""reasoning"": ""A rich summary (max 200 words) covering: Interior/Comfort (Seats, Climate, Roof), Technology (Display, CarPlay, Sound), Ownership (Maintenance, Resale Value), and why it fits the client.""
                        }}
                    ]
                }}
            ";

            var aiResponse = await SendGeminiRequest(prompt, true);

            try
            {
                var resultRoot = JsonConvert.DeserializeObject<RootAgentResponse>(aiResponse);

                return new AgentRecommendation
                {
                    AgentName = agentName,
                    AgentIcon = agentIcon,
                    BudgetStatus = budgetStatus,
                    Suggestions = resultRoot?.Suggestions ?? new List<CarSuggestion>(),
                    Recommendation = "See suggestions list"
                };
            }
            catch
            {
                // Fallback return if JSON parsing fails
                return new AgentRecommendation { AgentName = agentName, AgentIcon = agentIcon, BudgetStatus = budgetStatus };
            }
        }

        public async Task<JuryDecision> JuryDecision(List<AgentRecommendation> recommendations, UserRequest request)
        {
            // Serialize the agent recommendations to send to the Jury
            var agentDataJson = JsonConvert.SerializeObject(recommendations.Select(r => new {
                Agent = r.AgentName,
                Cars = r.Suggestions.Select(c => new {
                    Name = $"{c.Make} {c.Model} {c.Year}",
                    Price = c.Price,
                    Specs = c.Engine,
                    Safety = c.SafetyRating
                })
            }));

            var prompt = $@"
                ROLE: You are the HEAD AUTOMOTIVE JURY for the {request.Country} market.
                DATE: {DateTime.Now:MMMM yyyy}
                
                CLIENT PROFILE:
                - Budget: {request.MinBudget} - {request.MaxBudget} {request.Currency}
                - Needs: {request.Preferences}

                INPUT DATA (From your expert agents):
                {agentDataJson}

                TASK: Analyze the candidates and select the ONE absolute best car.

                SCORING CRITERIA (Be strict):
                - Engine & Performance (Max 15)
                - Safety (Max 20) - Prioritize Euro NCAP and Active Tech
                - Comfort & Interior (Max 15)
                - Technology (Max 10)
                - Value for Money (Max 15) - Price vs Features + Resale
                - Fuel Economy (Max 10)
                - Market Fit (Max 10) - Parts/Service availability in {request.Country}
                - Client Match (Max 5)

                OUTPUT FORMAT: Raw JSON only.
                {{
                    ""winningCar"": ""Brand Model Year"",
                    ""finalVerdict"": ""A comprehensive 4-5 sentence professional verdict explaining exactly why this car wins over the others, citing specific performance, safety, or value advantages."",
                    ""totalScore"": 88,
                    ""detailedScores"": {{ 
                        ""Performance"": 14, 
                        ""Safety"": 18, 
                        ""Comfort"": 12, 
                        ""Tech"": 8, 
                        ""Value"": 13, 
                        ""Fuel"": 9, 
                        ""MarketFit"": 9, 
                        ""ClientMatch"": 5 
                    }},
                    ""keyStrengths"": [
                        ""Specific Strength 1 (e.g. Best-in-class 0-100 acceleration of 6.5s)"", 
                        ""Specific Strength 2 (e.g. Only car with Matrix LED headlights in budget)"", 
                        ""Specific Strength 3""
                    ]
                }}
            ";

            var aiResponse = await SendGeminiRequest(prompt, true);

            try
            {
                var decision = JsonConvert.DeserializeObject<JuryDecision>(aiResponse);
                decision.Reasoning = decision.FinalVerdict;
                return decision;
            }
            catch
            {
                return new JuryDecision { WinningCar = "Error", FinalVerdict = "Analysis failed" };
            }
        }

        private async Task<string> SendGeminiRequest(string prompt, bool jsonMode)
        {
            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { responseMimeType = jsonMode ? "application/json" : "text/plain" }
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={_apiKey}", content);

            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseJson);

            return result.candidates[0].content.parts[0].text;
        }

        private class RootAgentResponse { public List<CarSuggestion> Suggestions { get; set; } }
    }
}