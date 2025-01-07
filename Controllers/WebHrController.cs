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

namespace WebHr.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WebHrController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WebHrController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("fetch_subordinates")]
        public async Task<IActionResult> FetchSubOrdinates()
        {

            
        }



    }
}
