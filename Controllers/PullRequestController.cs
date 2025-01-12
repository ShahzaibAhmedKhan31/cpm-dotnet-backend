using ApiRequest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace PullRequest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class PullRequestController : ControllerBase
    {

        private readonly PrService _prService;

        // Constructor to inject ElasticSearchService
        public PullRequestController(PrService prService)
        {
            _prService = prService;
        }

        [HttpPost("pr_count_by_month")]
        public async Task<IActionResult> GetPrCountByMonthApi([FromBody] SearchByUserDateRequest request)
        {
            
            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true ? rawEmail.Split('#').Last() : rawEmail;
            
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("CreatedByName and DateRange must be provided.");
            }

            try
            {   
                // GetPrCountByMonth
                var response = await _prService.GetPrCountByMonth(request.Date, request.UserName);

                // Return the formatted response
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("pr_with_comments_count")]
        public async Task<IActionResult> GetPrWithCommentsCountApi([FromBody] SearchByUserDateRequest request)
        {
            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true ? rawEmail.Split('#').Last() : rawEmail;

            Console.WriteLine("Username in pr_with_comments_count api:", name);
            Console.WriteLine("Email in pr_with_comments_count api: ", email);

            
            // Ensure the 'createdByName' and 'dateRangeStart' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("CreatedByName and Date Range must be provided.");
            }

            try
            {
                // getPrWithCommentsCount
                var response = await _prService.getPrWithCommentsCount(request.Date, request.UserName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("reviewed_pr_count")]
        public async Task<IActionResult> GetReviewedPrCountApi([FromBody] SearchByUserDateRequest request)
        {
            // Extract claims from the token
            var name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true ? rawEmail.Split('#').Last() : rawEmail;

            // Ensure the 'reviewerName' and 'dateRangeStart' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("ReviewerName and Date Range must be provided.");
            }

            try
            {
                // getReviewedPrCount
                var response = await _prService.getReviewedPrCount(request.Date, request.UserName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // For testing purpose only
        [HttpPost("get_pr_details_by_work_item_id")]
        public async Task<IActionResult> GetPrDetailsApi([FromBody] SearchPrDetails request)
        {
            try
                {
                    var getPrInsights = await _prService.GetPrInsights(request.WorkItemId);

                    return Ok(getPrInsights);

                }
            catch (Exception ex)
                {
                    // Return internal server error with exception details
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
        }
    }
}