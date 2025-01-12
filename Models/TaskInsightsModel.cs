// Models/TaskInsightsModel.cs
namespace TaskInsightsModel.Models
    {
    public class TaskInsights
    {
        public int? TaskId { get; set; }
        public int? OriginalEstimate { get; set; }
        public string? CreatedDate { get; set; }
        public object? BugsCount{get; set;}
        public string? EndDate { get; set; }
        public string? InProgressDate { get; set; }
        public string? PrCreatedByEmail { get; set; }
        public string? PrCreatedDate { get; set; }
        public string? PrCreatedByName { get; set; }
        public int PrId { get; set; }
        public string? PrTitle { get; set; }
        public string? PrTotalNumberOfComments { get; set; }
        public string? PrLastMergeCommitId { get; set; }
        public string? PrClosedDate { get; set; }
        public string? PrClosedByName { get; set; }
        public string? PrFirstCommentDate { get; set; }
        public string? PrStatus { get; set; }

    }


    public class WorkitemInsights
    {
        public int? TaskId { get; set; }
        public int? OriginalEstimate { get; set; }
        public string? CreatedDate { get; set; }
        public object? BugsCount{get; set;}
        public string? EndDate { get; set; }
        public string? InProgressDate { get; set; }

    }

}

