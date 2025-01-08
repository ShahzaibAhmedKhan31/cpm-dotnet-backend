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
        public int? Id { get; set; }

        public string? PrTitle { get; set; }

        public int PrCommentsCount { get; set; }
    }

    public class GetReviewedPrCountApiResponse
    {
        public string? Month { get; set; }

        public int PrReviewCount { get; set; }
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

    public class GetPrDetailsApi
    {
    public string? CreatedByEmail { get; set; }
    public string? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int WorkItemId { get; set; }
    public int PrId { get; set; }
    public string? Title { get; set; }
    public string? TotalNumberOfComments { get; set; }
    public string? LastMergeCommitId { get; set; }
    public string? PrClosedDate { get; set; }
    public string? PrClosedByName { get; set; }
    public string? PrFirstCommentDate { get; set; }
    public string? PrStatus { get; set; }

    }
	
	

}