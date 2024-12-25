// Models/SearchByIDRequest.cs
namespace ElasticsearchRequest.Models
{
    public class SearchByIDRequest
    {
        public string Index { get; set; } = "jira_completed_issues_new";  // default value
        public string? Id { get; set; }
    }

    public class SearchByUserDateRequest
    {
        public string Index { get; set; } = "jira_completed_issues_new";  // default value
        public string? UserName { get; set; }
        public string? Date { get; set; }
    }
}
