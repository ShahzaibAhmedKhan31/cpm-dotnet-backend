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

        // [HttpGet]
        // [Route("fetch_employee_details")]
        // public async Task<IActionResult> FetchEmployeeDetails([FromQuery] string email)
        // {
        //     var emp = await _empService.FetchEmployeeDetails(email);
            
        //     return Ok(emp);
    
        // }
        
        // [HttpGet]
        // [Route("get_department_id")]
        // public async Task<IActionResult> getId([FromQuery] string department_name)
        // {
        //     var id = await _empService.getDepartmentId(department_name);
        //     // var emp = await _empService.FetchEmployeeDetails(email);
            
        //     return Ok(id);
    
        // }
        



    }
}
