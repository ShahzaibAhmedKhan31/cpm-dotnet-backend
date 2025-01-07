using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.dbdata;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Route("api/task")]
    [ApiController]
    public class TaskScoreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaskScoreController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/task
        [HttpPost]
        public async Task<ActionResult<TaskScore>> PostTaskScore([FromBody] TaskScoreRequest taskScoreRequest)
        {
            if (taskScoreRequest == null)
            {
                return BadRequest("TaskScoreRequest cannot be null.");
            }

            // Ensure TaskId is provided
            if (string.IsNullOrEmpty(taskScoreRequest.taskid))
            {
                return BadRequest("TaskId is required.");
            }

            // Create a TaskScore object from the request data
            var taskScore = new TaskScore
            {
                taskid = taskScoreRequest.taskid,
                selfrating = taskScoreRequest.selfrating
            };

            // Since ScoreId is auto-incremented, we don't set it manually
            taskScore.scoreid = null;

            // Add the TaskScore to the database
            _context.TaskScores.Add(taskScore);
            await _context.SaveChangesAsync();

            // Return the created TaskScore with the auto-generated ScoreId
            return CreatedAtAction(nameof(PostTaskScore), new { id = taskScore.scoreid }, taskScore);
        }
    }

    // Create a DTO class to handle just the TaskId and SelfRating in the request body
    public class TaskScoreRequest
    {
        public string taskid { get; set; }
        public float selfrating { get; set; }
    }
}
