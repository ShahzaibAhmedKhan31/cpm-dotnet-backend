using JiraApi.Models;
using JiraApi.Services;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JiraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JiraController : ControllerBase
    {
        private readonly ElasticSearchService _elasticSearchService;
        private readonly HttpClient _httpClient;

        // Constructor that injects the Elasticsearch service and HttpClient
        public JiraController(ElasticSearchService elasticSearchService, HttpClient httpClient)
        {
            _elasticSearchService = elasticSearchService;
            _httpClient = httpClient;
        }

        // POST api/jira/search_by_id
        [HttpPost("search_by_id")]
        public async Task<IActionResult> SearchById([FromBody] SearchByIDRequest request)
        {
            // Ensure the 'index' and 'id' are provided in the request
            if (string.IsNullOrEmpty(request.Id))
            {
                return BadRequest("Id must be provided.");
            }

            // Search query to find the document by its id
            var response = await _elasticSearchService.Client.SearchAsync<object>(s => s
                .Index(request.Index) // Use the index provided in the request
                .Query(q => q
                    .Term(t => t
                        .Field("_id") // Search for the document with this ID
                        .Value(request.Id) // The value of the ID to search for
                    )
                )
            );

            // Check if the response was successful
            if (!response.IsValid || response.Documents.Count == 0)
            {
                return NotFound("No document found with the given ID.");
            }

            // Return the found document(s)
            return Ok(response.Documents);
        }

        [HttpPost("search_by_user_and_date")]
        public async Task<IActionResult> SearchByUserAndDate([FromBody] SearchByUserDateRequest request)
        {
            // Ensure the 'user_name' and 'date' are provided in the request
            if (string.IsNullOrEmpty(request.userName) || string.IsNullOrEmpty(request.date))
            {
                return BadRequest("UserName and Date must be provided.");
            }

            Console.WriteLine(request.userName);
            Console.WriteLine(request.date);
            Console.WriteLine(request.index);

            var query = new
            {
                query = new
                {
                    @bool = new
                    {
                        filter = new object[]
                        {
                            new
                            {
                                @bool = new
                                {
                                    should = new object[]
                                    {
                                        new { term = new { LEVEL_2_ASSIGNEE_keyword = request.userName } },
                                        new { term = new { LEVEL_3_ASSIGNEE_keyword = request.userName } },
                                        new { term = new { LEVEL_4_ASSIGNEE_keyword = request.userName } },
                                        new { term = new { LEVEL_5_ASSIGNEE_keyword = request.userName } }
                                    },
                                    minimum_should_match = 1
                                }
                            },
                            new
                            {
                                range = new
                                {
                                    TIMESTAMP = new
                                    {
                                        gte = request.date
                                    }
                                }
                            }
                        }
                    }
                }
            };


            // Serialize the query to JSON
            var queryJson = JsonSerializer.Serialize(query);

            // Replace the placeholders in the JSON string
            queryJson = queryJson.Replace("LEVEL_2_ASSIGNEE_keyword", "LEVEL_2_ASSIGNEE.keyword")
                                .Replace("LEVEL_3_ASSIGNEE_keyword", "LEVEL_3_ASSIGNEE.keyword")
                                .Replace("LEVEL_4_ASSIGNEE_keyword", "LEVEL_4_ASSIGNEE.keyword")
                                .Replace("LEVEL_5_ASSIGNEE_keyword", "LEVEL_5_ASSIGNEE.keyword");

                                
            Console.WriteLine(queryJson);

            // Elasticsearch endpoint
            var uri = $"http://localhost:9200/{request.index}/_search";

            // Send the request
            var httpContent = new StringContent(queryJson, Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync(uri, httpContent);

            // Process the response
            if (!httpResponse.IsSuccessStatusCode)
            {
                var error = await httpResponse.Content.ReadAsStringAsync();
                return StatusCode((int)httpResponse.StatusCode, error);
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            Console.WriteLine(responseContent);
            
            return Ok(JsonSerializer.Deserialize<object>(responseContent));
        }
    }
}