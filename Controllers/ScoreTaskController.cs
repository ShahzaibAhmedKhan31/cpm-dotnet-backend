// Controllers/ElasticSearchController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace ScoreTaskApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class ScoreTaskController : ControllerBase
    {
        private readonly ScoretaskService _scoretaskService;

        public ScoreTaskController(ScoretaskService scoretaskService)
        {
             _scoretaskService = scoretaskService;

        }

        [HttpPost]
        [Route("getTaskScore")]
        public async Task<IActionResult> GetTaskScore([FromBody] JsonElement request)
            {
                try{
                    var response = _scoretaskService.GetTaskScore(request);

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        
        [HttpPost]
        [Route("updateTask")]

        public async Task<IActionResult> UpdateTask([FromBody] JsonElement request, [FromQuery] string taskId)
        {
            try
            {
                var response = await _scoretaskService.UpdateTask(request, taskId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}