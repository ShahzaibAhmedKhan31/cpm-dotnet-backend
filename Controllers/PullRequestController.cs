using ElasticsearchRequest.Models;
using ApiResponse.Models;
using Microsoft.AspNetCore.Mvc;

namespace PullRequest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PullRequestController : ControllerBase
    {

        private readonly ElasticSearchService _elasticSearchService;

        // Constructor to inject ElasticSearchService
        public PullRequestController(ElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        [HttpPost("pr_count_by_month")]
        public async Task<IActionResult> GetPrCountByMonthApi([FromBody] SearchByUserDateRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("CreatedByName and DateRange must be provided.");
            }

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
                                    ""CREATED_BY_NAME.keyword"": ""{request.UserName}""
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
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, request.Index);

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
            // Ensure the 'createdByName' and 'dateRangeStart' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("CreatedByName and Date Range must be provided.");
            }

            var query = $@"
            {{
            ""_source"": [""LAST_MERGE_COMMIT_ID"", ""TOTAL_NUMBER_OF_COMMENTS"", ""PR_TITLE""],
            ""query"": {{
                ""bool"": {{
                ""must"": [
                    {{
                    ""term"": {{
                        ""CREATED_BY_NAME.keyword"": ""{request.UserName}""
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
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, request.Index);

                // Parse the response to extract PR details
                var prDetails = response
                    .GetProperty("hits")
                    .GetProperty("hits")
                    .EnumerateArray()
                    .Select(hit => new GetPrWithCommentsCountApiResponse
                    {
                        Id = hit.GetProperty("_source").GetProperty("LAST_MERGE_COMMIT_ID").GetString(),
                        PrCommentsCount = hit.GetProperty("_source").GetProperty("TOTAL_NUMBER_OF_COMMENTS").GetInt32(),
                        PrTitle = hit.GetProperty("_source").GetProperty("PR_TITLE").GetString()
                    })
                    .ToList();

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
                        ""CLOSED_BY_NAME.keyword"": ""{request.UserName}""
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
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, request.Index);

                // Parse the response to extract the pr_count by month
                var monthlyData = response
                    .GetProperty("aggregations")
                    .GetProperty("reviewed_pr_count_by_month")
                    .GetProperty("buckets")
                    .EnumerateArray()
                    .Select(bucket => new GetReviewedPrCountApi
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

    }
}