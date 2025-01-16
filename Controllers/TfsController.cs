// Controllers/ElasticSearchController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace TfsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class TfsController : ControllerBase
    {
        private readonly TfsService _tfsService;

        public TfsController(TfsService tfsService)
        {
            _tfsService = tfsService;

        }

        [HttpPost]
        [Route("bugs_count")]
        public async Task<IActionResult> BugsCountApi([FromBody] SearchRequest request)
        {

            var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            Console.WriteLine("Username: " + username);
            Console.WriteLine("Email: " + rawEmail);
            var email = rawEmail?.Contains("#") == true
                ? rawEmail.Split('#').Last()
                : rawEmail;

            try
            {
                var response = await _tfsService.getBugsCount(username, request.Months);

                // Return the processed buckets
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("task_count")]
        public async Task<IActionResult> TaskCount([FromBody] SearchRequest request)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true
                ? rawEmail.Split('#').Last()
                : rawEmail;

            try
            {
                var response = await _tfsService.getTaskCount(username, request.Months);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }



        }

        [HttpPost]
        [Route("task_completion_rate")]
        public async Task<IActionResult> TaskCompletionRate([FromBody] SearchRequest request)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true
                ? rawEmail.Split('#').Last()
                : rawEmail;

            Console.WriteLine($"task_completion_rate {email}");

            try
            {
                var response = await _tfsService.getTaskCompletionRate(username, request.Months);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("workitem_bug_count")]
        public async Task<IActionResult> WorkItemBugCount([FromBody] SearchRequest request)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true
                ? rawEmail.Split('#').Last()
                : rawEmail;

            try
            {
                var response = await _tfsService.getWorkItemBugCount(username, request.Months);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log and return the error response
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }

        }

        [HttpGet]
        [Route("workitems")]
        public async Task<IActionResult> GetWorkItems([FromQuery] int month)
        {

            var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true
                ? rawEmail.Split('#').Last()
                : rawEmail;

            Console.WriteLine($"workitems ${username} ${email}");    

            // var email = "hamza01961@gmail.com";


            try
            {

                var response = await _tfsService.getWorkItems(username, month);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the error and return a bad request response
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }

        }

        [HttpPost]  // Change to POST method
        [Route("postworkitems")]
        public async Task<IActionResult> GetWorkItems([FromBody] WorkItemsRequest request)
        {
            try
            {
                // You can access the email and month directly from the request body
                var email = request.Email;
                var month = request.Month;

                var response = await _tfsService.getWorkItems("shahzaib_pakistan@hotmail.com", month);

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the error and return a bad request response
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("workiteminsights")]
        public async Task<IActionResult> GetWorkItemInsights([FromQuery] int work_item_id)
        {
            try
            {
                var response = await _tfsService.TfsInsights(work_item_id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("workitem_count_byEmail")]
        public async Task<IActionResult> WorkItemCountByEmail([FromBody] TaskCountRequest request)
        {
            try
            {
                // Call the service with the provided email list and month
                var response = await _tfsService.GetWorkItemCountByEmail(request.EmailList, request.Month, "Task");

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("bug_count_ByEmail")]
        public async Task<IActionResult> BugCountByEmail([FromBody] TaskCountRequest request)
        {

            try
            {
                var response = await _tfsService.GetWorkItemCountByEmail(request.EmailList, request.Month, "Bug");


                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }



        }

        [HttpPost]
        [Route("completion_rateByEmail")]
        public async Task<IActionResult> CompletionRateByEmail([FromBody] TaskCountRequest request)
        {

            try
            {
                var response = await _tfsService.GetWorkItemCompletionRateByEmail(request.EmailList, request.Month);


                return Ok(response);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }



        }


    }

    // Model for the request body
    public class TaskCountRequest
    {
        public List<string> EmailList { get; set; }
        public int Month { get; set; }
    }
    public class WorkItemsRequest
    {
        public string Email { get; set; }
        public int Month { get; set; }
    }
}
