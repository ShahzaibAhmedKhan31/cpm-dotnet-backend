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

            Console.WriteLine("Username: "+username);
            Console.WriteLine("Email: "+rawEmail);
            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;

            try
            {
                var response = await _tfsService.getBugsCount(email, request.Months);

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

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;

            try
            {
                var response = await _tfsService.getTaskCount(email, request.Months);

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

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;


            try
            {
                var response = await _tfsService.getTaskCompletionRate(email, request.Months);

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

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;

            try
            {
                var response = await _tfsService.getWorkItemBugCount(email, request.Months);

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
        

        var rawEmail = User.Identity?.Name;

        var email = rawEmail?.Contains("#") == true 
            ? rawEmail.Split('#').Last() 
            : rawEmail;

        // var email = "hamza01961@gmail.com";


        try{

            var response = await _tfsService.getWorkItems(email, month);

            return Ok(response);
        }
        catch (Exception ex){
            // Log the error and return a bad request response
            return BadRequest($"Error querying Elasticsearch: {ex.Message}");
        }
    
    }

    [HttpGet]
    [Route("workiteminsights")]
    public async Task<IActionResult> GetWorkItemInsights([FromQuery] int work_item_id)
        {
            try{
                var response = await _tfsService.TfsInsights(work_item_id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
