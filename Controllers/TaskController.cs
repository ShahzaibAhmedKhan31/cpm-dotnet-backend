using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.dbdata;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        // GET: api/task/{taskid}
        [HttpGet("{taskid}")]
        public async Task<ActionResult<TaskScore>> GetTaskScore(string taskid)
        {
            if (string.IsNullOrEmpty(taskid))
            {
                return BadRequest("TaskId is required.");
            }

            // Fetch the TaskScore for the given TaskId
            var taskScore = await _context.TaskScores
                                          .FirstOrDefaultAsync(ts => ts.taskid == taskid);

            if (taskScore == null)
            {
                return NotFound($"No TaskScore found for TaskId: {taskid}");
            }

            // Return the TaskScore
            return Ok(taskScore);
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

    // Check if the task already exists in the database
    var existingTaskScore = await _context.TaskScores
                                          .FirstOrDefaultAsync(ts => ts.taskid == taskScoreRequest.taskid);

    if (existingTaskScore != null)
    {
        // Update the existing record
        existingTaskScore.selfrating = taskScoreRequest.selfrating;
        _context.TaskScores.Update(existingTaskScore);
        await _context.SaveChangesAsync();

        // Return the updated TaskScore
        return Ok(existingTaskScore);
    }
    else
    {
        // Create a new TaskScore record
        var taskScore = new TaskScore
        {
            taskid = taskScoreRequest.taskid,
            selfrating = taskScoreRequest.selfrating,
            scoreid = null // ScoreId is auto-incremented
        };

        _context.TaskScores.Add(taskScore);
        await _context.SaveChangesAsync();

        // Return the newly created TaskScore
        return CreatedAtAction(nameof(PostTaskScore), new { id = taskScore.scoreid }, taskScore);
    }
}
    }

    // Create a DTO class to handle just the TaskId and SelfRating in the request body
    public class TaskScoreRequest
    {
        public string taskid { get; set; }
        public float selfrating { get; set; }
    }
}
