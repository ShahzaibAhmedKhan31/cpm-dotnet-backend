// Controllers/ElasticSearchController.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ApiRequest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebApplication1.Models;
using WebApplication1.dbdata;
using Microsoft.EntityFrameworkCore;  // Add this


namespace Employees.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EmployeesService _empService;
        private readonly ApplicationDbContext _context;

        public EmployeesController(IHttpClientFactory httpClientFactory, EmployeesService empService, ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _empService = empService;
            _context = context;
        }

        [HttpGet]
        [Route("fetch_subordinates")]
        public async Task<IActionResult> FetchSubOrdinates([FromQuery] string email)
        {
            var emp = await _empService.fetchSubordinates(email);
            
            return Ok(emp);
            
        }



    }
}
