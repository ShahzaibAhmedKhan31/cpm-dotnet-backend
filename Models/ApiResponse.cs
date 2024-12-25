// Models/ApiResponse.cs
namespace ApiResponse.Models
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

    public class GetPrCountByMonthApiResponse
    {
        public string? Month { get; set; }
        public int PrCount { get; set; }
    }

    public class GetPrWithCommentsCountApiResponse
    {
        public string? Id { get; set; }

        public string? PrTitle { get; set; }

        public int PrCommentsCount { get; set; }
    }

    public class GetReviewedPrCountApi
    {
        public string? Month { get; set; }

        public int PrReviewCount { get; set; }
    }
}