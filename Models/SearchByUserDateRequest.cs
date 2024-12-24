// Models/SearchByUserDateRequest.cs
namespace JiraApi.Models
{
    public class SearchByUserDateRequest
    {
        public string index { get; set; } = "jira_completed_issues_new";  // default value
        public string userName { get; set; }
        public string date { get; set; }
    }
}
