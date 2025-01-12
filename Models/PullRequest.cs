// Models/TaskInsightsModel.cs
namespace PullRequest.Models
{
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
}

