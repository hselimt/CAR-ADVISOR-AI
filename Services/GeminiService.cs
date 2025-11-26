using CarAdvisorAPI.Models;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace CarAdvisorAPI.Services
{
    public interface IGeminiService
    {
        Task<AgentRecommendation> CallAgent(string agentName, string agentIcon, string brands, string focus, UserRequest request, string budgetStatus);
        Task<JuryDecision> JuryDecision(List<AgentRecommendation> recommendations, UserRequest request);
        Task<string?> GenerateCarImage(string carName);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiApiKey"] ?? "";
        }

        public async Task<AgentRecommendation> CallAgent(string agentName, string agentIcon, string brands, string focus, UserRequest request, string budgetStatus)
        {
            var currentDate = DateTime.Now.ToString("MMMM yyyy");
            var currentYear = DateTime.Now.Year;
            var minCarYear = currentYear - 8;

            string budgetContext = budgetStatus switch
            {
                "too-expensive" => "WARNING: Your segment is ABOVE the client's budget. Acknowledge this but recommend the closest options or used alternatives.",
                "too-cheap" => "WARNING: Your segment is BELOW the client's budget. Acknowledge the client can afford more premium cars.",
                _ => "Your segment fits the budget."
            };

            string turkeyContext = request.Country == "Turkey" ? @"
TURKEY MARKET RULES (VERY IMPORTANT):
- ONLY recommend models that are COMMONLY SOLD in Turkey with official dealership networks
- Popular brands in Turkey: Toyota, Honda, Hyundai, Kia, Volkswagen, Skoda, Renault, Fiat, Ford, Mercedes, BMW, Audi
- AVOID rare/uncommon models like: Genesis, Infiniti, Cadillac, Chrysler, Dodge, US-spec vehicles
- Focus on models with widespread service network and parts availability in Turkey
- Turkish consumers prefer: fuel efficiency, low maintenance costs, resale value
" : "";

            var prompt = $@"You are a professional automotive expert specializing in {focus} vehicles for the {request.Country} market.

CONTEXT:
- Current Date: {currentDate}
- Target Market: {request.Country}
- Client Budget: {request.MinBudget} - {request.MaxBudget} {request.Currency} (STRICT MAXIMUM - DO NOT EXCEED)
- Client Preferences: {request.Preferences}
- Your Specific Brands: {brands}
- Budget Status: {budgetContext}
{turkeyContext}
TASK: Recommend 2-3 specific vehicles ({minCarYear}-{currentYear} models) currently available in {request.Country}.

CRITICAL REQUIREMENTS:
1. ALL PRICES MUST BE UNDER {request.MaxBudget} {request.Currency} - THIS IS A HARD LIMIT
2. USE REAL MARKET PRICES for {request.Country} in {currentDate}
3. DETAILED SPECS: HP, Torque, 0-100 km/h times
4. SAFETY: NCAP ratings and safety features
5. OWNERSHIP: Fuel consumption and maintenance costs

OUTPUT: Return ONLY valid JSON, no markdown, no explanation:
{{
    ""suggestions"": [
        {{
            ""make"": ""Brand Name"",
            ""model"": ""Model Name"",
            ""year"": ""2024"",
            ""price"": ""45,000 {request.Currency}"",
            ""engine"": ""Engine specs: Type, CC, HP, Torque, 0-100, Transmission, Drive"",
            ""fuel"": ""Fuel type, tank size, consumption L/100km, monthly cost"",
            ""safetyRating"": ""NCAP stars, airbags, safety tech"",
            ""reasoning"": ""Why this car fits the client (max 150 words)""
        }}
    ]
}}";

            var aiResponse = await SendGeminiRequest(prompt);

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
            catch (Exception ex)
            {
                Console.WriteLine($"JSON Parse Error for {agentName}: {ex.Message}");
                Console.WriteLine($"Response was: {aiResponse}");
                return new AgentRecommendation { AgentName = agentName, AgentIcon = agentIcon, BudgetStatus = budgetStatus };
            }
        }

        public async Task<JuryDecision> JuryDecision(List<AgentRecommendation> recommendations, UserRequest request)
        {
            var agentDataJson = JsonConvert.SerializeObject(recommendations.Select(r => new
            {
                Agent = r.AgentName,
                Cars = r.Suggestions.Select(c => new
                {
                    Name = $"{c.Make} {c.Model} {c.Year}",
                    Price = c.Price,
                    Specs = c.Engine,
                    Safety = c.SafetyRating
                })
            }), Formatting.Indented);

            var prompt = $@"You are the HEAD AUTOMOTIVE JURY for the {request.Country} market.
DATE: {DateTime.Now:MMMM yyyy}

CLIENT PROFILE:
- Budget: {request.MinBudget} - {request.MaxBudget} {request.Currency}
- Needs: {request.Preferences}

CANDIDATES FROM AGENTS:
{agentDataJson}

TASK: Select the ONE absolute best car for this client.

SCORING (Max 100):
- Performance: /15
- Safety: /20
- Comfort: /15
- Technology: /10
- Value: /15
- Fuel Economy: /10
- Market Fit: /10
- Client Match: /5

OUTPUT: Return ONLY valid JSON, no markdown:
{{
    ""winningCar"": ""Brand Model Year"",
    ""winningCarPrice"": ""45,000 {request.Currency}"",
    ""finalVerdict"": ""4-5 sentence verdict explaining why this car wins"",
    ""totalScore"": 85,
    ""detailedScores"": {{
        ""Performance"": 13,
        ""Safety"": 18,
        ""Comfort"": 12,
        ""Tech"": 8,
        ""Value"": 12,
        ""Fuel"": 8,
        ""MarketFit"": 9,
        ""ClientMatch"": 5
    }},
    ""keyStrengths"": [""Strength 1"", ""Strength 2"", ""Strength 3""]
}}";

            var aiResponse = await SendGeminiRequest(prompt);

            try
            {
                var decision = JsonConvert.DeserializeObject<JuryDecision>(aiResponse);
                if (decision != null)
                {
                    decision.Reasoning = decision.FinalVerdict;
                    return decision;
                }
                return new JuryDecision { WinningCar = "Error", FinalVerdict = "Parse failed" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Jury JSON Parse Error: {ex.Message}");
                return new JuryDecision { WinningCar = "Error", FinalVerdict = "Analysis failed" };
            }
        }

        private async Task<string> SendGeminiRequest(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 4096,
                    responseMimeType = "application/json"
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"{GEMINI_API_URL}?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API Error: {response.StatusCode} - {errorBody}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(responseJson)!;

            string text = result.candidates[0].content.parts[0].text;
            text = CleanJsonResponse(text);

            return text;
        }

        private string CleanJsonResponse(string text)
        {
            text = Regex.Replace(text, @"```json\s*", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"```\s*", "");
            text = text.Trim();
            return text;
        }

        public async Task<string?> GenerateCarImage(string carName)
        {
            try
            {
                var prompt = $"A clean, modern photo of a {carName} car, 3/4 front view, studio lighting, dark gray background, professional automotive photography";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        responseModalities = new[] { "IMAGE", "TEXT" }
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Try gemini-2.0-flash-exp for image generation
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Image Generation Error: {response.StatusCode} - {errorBody}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseJson)!;

                // Extract base64 image from response
                var parts = result.candidates[0].content.parts;
                foreach (var part in parts)
                {
                    if (part.inlineData != null)
                    {
                        string base64Image = part.inlineData.data;
                        string mimeType = part.inlineData.mimeType;
                        return $"data:{mimeType};base64,{base64Image}";
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Image Generation Exception: {ex.Message}");
                return null;
            }
        }

        private class RootAgentResponse
        {
            public List<CarSuggestion> Suggestions { get; set; } = new();
        }
    }
}
