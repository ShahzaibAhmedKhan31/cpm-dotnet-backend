// Controllers/ElasticSearchController.cs
using Microsoft.AspNetCore.Mvc;

namespace Employees.Controllers
{
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeesService _empService;

        public EmployeesController(EmployeesService empService)
        {
            _empService = empService;
        }

        [HttpGet]
        [Route("fetch_subordinates")]
        public async Task<IActionResult> FetchSubOrdinates([FromQuery] string email)
        {
            var emp = await _empService.FetchSubordinates(email);
            
            return Ok(emp);
            
        }



    }
}
