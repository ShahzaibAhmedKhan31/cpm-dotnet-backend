using ApiRequest.Models;
using ApiResponse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace PullRequest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PullRequestController : ControllerBase
    {

        private readonly ElasticSearchService _elasticSearchService;

        private readonly PrService _prService;
        private readonly string _indexName;

        // Constructor to inject ElasticSearchService
        public PullRequestController(ElasticSearchService elasticSearchService, IOptions<IndexesName> settings, PrService prService)
        {
            _prService = prService;
            _elasticSearchService = elasticSearchService;
            _indexName = settings.Value.PR;
            
            
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

            // var rawEmail = User.Identity?.Name;

            // Console.WriteLine("PR controller:Email: "+rawEmail);
            // var email = rawEmail?.Contains("#") == true 
            //     ? rawEmail.Split('#').Last() 
            //     : rawEmail;

            var query = $@"
            {{
                ""query"": {{
                    ""bool"": {{
                        ""must"": [
                            {{
                                ""term"": {{
                                    ""PR_STATUS.keyword"": ""completed""
                                }}
                            }},
                            {{
                                ""term"": {{
                                    ""CREATED_BY_NAME.keyword"": ""{name}""
                                }}
                            }}
                        ],
                        ""filter"": [
                            {{
                                ""range"": {{
                                    ""PR_CLOSE_DATE"": {{
                                        ""gte"": ""{request.Date}""
                                    }}
                                }}
                            }}
                        ]
                    }}
                }},
                ""aggs"": {{
                    ""pr_count_by_month"": {{
                        ""date_histogram"": {{
                            ""field"": ""PR_CLOSE_DATE"",
                            ""calendar_interval"": ""month"",
                            ""format"": ""yyyy-MM""
                        }}
                    }}
                }},
                ""size"": 0
            }}";

            try
            {   

                // Execute Elasticsearch query
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

                 // Parse the response to extract the pr_count by month
                var monthlyData = response
                    .GetProperty("aggregations")
                    .GetProperty("pr_count_by_month")
                    .GetProperty("buckets")
                    .EnumerateArray()
                    .Select(bucket => new GetPrCountByMonthApiResponse
                    {
                        Month = bucket.GetProperty("key_as_string").GetString(),
                        PrCount = bucket.GetProperty("doc_count").GetInt32()
                    })
                    .ToList();

                // Return the formatted response
                return Ok(monthlyData);
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

            var query = $@"
            {{
            ""_source"": [""PR_ID"", ""TOTAL_NUMBER_OF_COMMENTS"", ""PR_TITLE""],
            ""query"": {{
                ""bool"": {{
                ""must"": [
                    {{
                    ""term"": {{
                        ""CREATED_BY_NAME.keyword"": ""{name}""
                    }}
                    }}
                ],
                ""filter"": [
                    {{
                    ""range"": {{
                        ""PR_CREATION_DATE"": {{
                        ""gte"": ""{request.Date}"",
                        ""lte"": ""now/M""
                        }}
                    }}
                    }}
                ]
                }}
            }},
            ""size"": 100
            }}";

            try
            {

                // Execute Elasticsearch query
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

                Console.WriteLine("Elastic Search response: ", response);

                // Parse the response to extract PR details
                var prDetails = response
                    .GetProperty("hits")
                    .GetProperty("hits")
                    .EnumerateArray()
                    .Select(hit => new GetPrWithCommentsCountApiResponse
                    {
                        Id = hit.GetProperty("_source").GetProperty("PR_ID").GetInt32(),
                        PrCommentsCount = hit.GetProperty("_source").GetProperty("TOTAL_NUMBER_OF_COMMENTS").GetInt32(),
                        PrTitle = hit.GetProperty("_source").GetProperty("PR_TITLE").GetString()
                    })
                    .ToList();

                Console.WriteLine(prDetails);
                // Return the formatted response
                return Ok(prDetails);
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

            var query = $@"
            {{
            ""query"": {{
                ""bool"": {{
                ""must"": [
                    {{
                    ""term"": {{
                        ""CLOSED_BY_NAME.keyword"": ""{name}""
                    }}
                    }}
                ],
                ""filter"": [
                    {{
                    ""range"": {{
                        ""PR_CLOSE_DATE"": {{
                        ""gte"": ""{request.Date}"",
                        ""lte"": ""now/M""
                        }}
                    }}
                    }}
                ]
                }}
            }},
            ""aggs"": {{
                ""reviewed_pr_count_by_month"": {{
                ""date_histogram"": {{
                    ""field"": ""PR_CLOSE_DATE"",
                    ""calendar_interval"": ""month"",
                    ""format"": ""yyyy-MM""
                }}
                }}
            }},
            ""size"": 0
            }}";

            try
            {
                
                // Execute Elasticsearch query
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

                // Parse the response to extract the pr_count by month
                var monthlyData = response
                    .GetProperty("aggregations")
                    .GetProperty("reviewed_pr_count_by_month")
                    .GetProperty("buckets")
                    .EnumerateArray()
                    .Select(bucket => new GetReviewedPrCountApiResponse
                    {
                        Month = bucket.GetProperty("key_as_string").GetString(),
                        PrReviewCount = bucket.GetProperty("doc_count").GetInt32()
                    })
                    .ToList();

                // Return the formatted response
                return Ok(monthlyData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

[HttpPost("get_pr_details_by_work_item_id")]
public async Task<IActionResult> GetPrDetailsApi([FromBody] SearchPrDetails request)
{
    try
    {
        // Get PR ID query based on work item ID
        var get_pr_id_query = _prService.getPrIdQuery(request.WorkItemId);

        // Execute the Elasticsearch query to get PR ID
        var response_1 = await _elasticSearchService.ExecuteElasticsearchQueryAsync(get_pr_id_query, _indexName);

        var total_1 = response_1
            .GetProperty("hits")
            .GetProperty("total")
            .GetProperty("value").GetInt32();

        // Check if any hits are returned
        if (total_1 > 0)
        {
            // Extract the PR data from the first response
            var workItemPrData = _prService.getWorkItemPrData(response_1);

            // Ensure that workItemPrData contains data before proceeding
            if (workItemPrData.Any())
            {
                // Get the PR details query based on the first item
                var get_pr_details_query = _prService.GetPrDetailsQuery(workItemPrData[0].PrId);

                // Execute Elasticsearch query for PR details
                var response_2 = await _elasticSearchService.ExecuteElasticsearchQueryAsync(get_pr_details_query, _indexName);

                var total_2 = response_2
                    .GetProperty("hits")
                    .GetProperty("total")
                    .GetProperty("value").GetInt32();

                // Return the response if PR details are found
                if (total_2 > 0)
                {
                    var pr_details = _prService.getPrDetails(response_2);

                    var createPrDetailsResponse = new GetPrDetailsApi 
                    {
                        CreatedByEmail = workItemPrData[0].CreatedByEmail,
                        CreatedDate = workItemPrData[0].CreatedDate,
                        CreatedByName = workItemPrData[0].CreatedByName,
                        WorkItemId = workItemPrData[0].WorkItemId,
                        PrId = workItemPrData[0].PrId,
                        Title = workItemPrData[0].Title,
                        TotalNumberOfComments = pr_details.TotalNumberOfComments.ToString(),
                        LastMergeCommitId = pr_details.LastMergeCommitId,
                        PrClosedDate = pr_details.PrClosedDate,
                        PrClosedByName = pr_details.PrClosedByName,
                        PrFirstCommentDate = pr_details.PrFirstCommentDate,
                        PrStatus = pr_details.PrStatus
                    };

                    return Ok(createPrDetailsResponse);
                }
                else
                {

                    var createPrDetailsResponse = new GetPrDetailsApi 
                    {
                        CreatedByEmail = workItemPrData[0].CreatedByEmail,
                        CreatedDate = workItemPrData[0].CreatedDate,
                        CreatedByName = workItemPrData[0].CreatedByName,
                        WorkItemId = workItemPrData[0].WorkItemId,
                        PrId = workItemPrData[0].PrId,
                        Title = workItemPrData[0].Title,
                        TotalNumberOfComments = "",
                        LastMergeCommitId = "",
                        PrClosedDate = "",
                        PrClosedByName = "",
                        PrFirstCommentDate = "",
                        PrStatus = "In-Progress"
                    };
                    return Ok(createPrDetailsResponse);
                }
            }
            else
            {
                return Ok(new { message = "No work item data found." });
            }
        }
        else
        {
            return Ok(new { message = "No PR found against this WorkItem." });
        }
    }
    catch (Exception ex)
    {
        // Return internal server error with exception details
        return StatusCode(500, $"Internal server error: {ex.Message}");
    }
}

    }
}