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

namespace Employees.Controllers
{
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EmployeesService _empService;

        public EmployeesController(IHttpClientFactory httpClientFactory, EmployeesService empService)
        {
            _httpClientFactory = httpClientFactory;
            _empService = empService;
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
