namespace CarAdvisorAPI.Models
{
    public class UserRequest
    {
        public string MinBudget { get; set; } = string.Empty;
        public string MaxBudget { get; set; } = string.Empty;
        public string Preferences { get; set; } = string.Empty;
        public string Country { get; set; } = "Turkey";
        public string Currency { get; set; } = "TRY";
    }

    public class CarSuggestion
    {
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Engine { get; set; } = string.Empty;
        public string Fuel { get; set; } = string.Empty;
        public string SafetyRating { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
    }

    public class AgentRecommendation
    {
        public string AgentName { get; set; } = string.Empty;
        public string AgentIcon { get; set; } = string.Empty;
        public List<CarSuggestion> Suggestions { get; set; } = new();
        public string BudgetStatus { get; set; } = "in-budget";
        public string Recommendation { get; set; } = string.Empty;
    }

    public class AnalysisResult
    {
        public List<AgentRecommendation> Recommendations { get; set; } = new();
        public JuryDecision? Winner { get; set; }
    }

    public class JuryDecision
    {
        public string WinningCar { get; set; } = string.Empty;
        public string FinalVerdict { get; set; } = string.Empty;
        public int TotalScore { get; set; }
        public Dictionary<string, int> DetailedScores { get; set; } = new();
        public List<string> KeyStrengths { get; set; } = new();
        public string Reasoning { get; set; } = string.Empty;
        public List<string> RunnerUps { get; set; } = new();
    }
}