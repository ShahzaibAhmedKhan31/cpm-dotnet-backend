// Models/SearchByIDRequest.cs
namespace JiraApi.Models
{
    public class SearchByIDRequest
    {
        public string Index { get; set; } = "jira_completed_issues_new";  // default value
        public string Id { get; set; }
    }
}
