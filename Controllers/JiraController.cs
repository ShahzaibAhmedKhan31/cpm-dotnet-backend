using ApiRequest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace JiraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class JiraController : ControllerBase
    {

        private readonly JiraService _jiraService;

        // Constructor to inject ElasticSearchService
        public JiraController(JiraService jiraService)
        {
            _jiraService = jiraService;
            
        }

        // POST api/jira/completed_and_breached
        [HttpPost("completed_and_breached")]
        public async Task<IActionResult> CompletedAndBreachedApi([FromBody] SearchByUserDateRequest request)
        {

            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true ? rawEmail.Split('#').Last() : rawEmail;

            // Ensure the 'user_name' and 'date' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("UserName and Date must be provided.");
            }
            
            try
            {
                // getCompletedAndBreachedIssues
                var response = await _jiraService.getCompletedAndBreachedIssues(request.Date, request.UserName);

                // Return the extracted data
                return Ok(response);
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

        // POST api/jira/breached_and_non_breached
        [HttpPost("breached_and_non_breached")]
        public async Task<IActionResult> BreachedAndNonBreachedApi([FromBody] SearchByUserDateRequest request)
        {
            
            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true ? rawEmail.Split('#').Last() : rawEmail;

            // Ensure the 'user_name' and 'date' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("UserName and Date must be provided.");
            }

            try
                {
                
                // getBreachedAndNonBreachedIssues
                var response = await _jiraService.getBreachedAndNonBreachedIssues(request.Date,request.UserName);

                return Ok(response);
            }
            catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
        }


        // POST api/jira/completionrate
        [HttpPost("completionrate")]
        public async Task<IActionResult> CompletionRateApi([FromBody] SearchByUserDateRequest request)
        {
            
            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true ? rawEmail.Split('#').Last() : rawEmail;

            // Ensure the 'user_name' and 'date' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("UserName and Date must be provided.");
            }

            try
            {
                
                // getCompletionRate
                var response = await _jiraService.getCompletionRate(request.Date, request.UserName);

                // Return the formatted monthly data
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // POST api/jira/completed_issue_by_id
        [HttpGet("completed_issues_list")]
        public async Task<IActionResult> CompletedIssueListApi([FromQuery] int month)
        {

            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true ? rawEmail.Split('#').Last() : rawEmail;

            
            string username = "Yusma Rasheed";
            // Ensure the 'id' is provided in the request
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest("Username must be provided.");
            }

            try
            {
                // getCompletedIssueList
                var response = await _jiraService.getCompletedIssueList(month, username);

                // Return the result
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST api/jira/completed_issue_by_id
        [HttpPost("getJiraInsights")]
        public async Task<IActionResult> getJiraInsights([FromBody] SearchJiraIssueDetails request)
        {
            try{
                // getJiraInsights
                var getJiraInsights = await _jiraService.getJiraInsights(request.IssueKey, request.DisplayName);

                return Ok(getJiraInsights);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}