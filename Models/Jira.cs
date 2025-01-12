// Models/TaskInsightsModel.cs
namespace Jira.Models
{
    public class CompletedAndBreachedApi
        {
            public string? Month { get; set; }
            public int CompletedIssuesCount { get; set; }
            public int BreachedIssuesCount { get; set; }
        }

    public class BreachedAndNonBreachedApiResponse
        {
            public int TotalIssuesBreachCount { get; set; }
            public int TotalIssuesNonBreachCount { get; set; }
        }

    public class CompletionRateApiResponse
        {
            public string? Month { get; set; }
            public int TotalIssuesNonBreachCount { get; set; }
            public int TotalIssuesCompleted { get; set; }
            public double CompletionRate { get; set; }
        }
    public class GetCompletedIssueListApiResponse
    {
        public string? Id {get; set;}
        public string? IssueKey {get; set;}
        public string? Date {get; set;}
        public string? Status {get; set;}
        public string? Summary {get; set;}
        public string? Severity {get; set;}
        public string? L2_breach {get; set;}
        public string? L3_breach {get; set;}
        public string? L4_breach {get; set;}
        public string? L5_breach {get; set;}

    }
}

