using ApiRequest.Models;
using ApiResponse.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace JiraApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JiraController : ControllerBase
    {
        private readonly ElasticSearchService _elasticSearchService;

        // Constructor to inject ElasticSearchService
        public JiraController(ElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        // POST api/jira/completed_issue_by_id
        [HttpPost("completed_issue_by_id")]
        [Authorize]
        public async Task<IActionResult> CompletedIssueByIdApi([FromBody] SearchByIDRequest request)
        {
            // Ensure the 'id' is provided in the request
            if (string.IsNullOrEmpty(request.Id))
            {
                return BadRequest("ID must be provided.");
            }

            var query = $@"
            {{
                ""query"": {{
                    ""bool"": {{
                        ""filter"": [
                            {{
                                ""term"": {{
                                    ""_id"": ""{request.Id}""
                                }}
                            }}
                        ]
                    }}
                }}
            }}";

            try
            {
                // Execute Elasticsearch query
                if (string.IsNullOrEmpty(request.Index))
                {
                    return BadRequest("Index must be provided.");
                }

                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, request.Index);

                // Return the result
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // POST api/jira/completed_and_breached
        [HttpPost("completed_and_breached")]
        [Authorize]
        public async Task<IActionResult> CompletedAndBreachedApi([FromBody] SearchByUserDateRequest request)
        {
            // Ensure the 'user_name' and 'date' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("UserName and Date must be provided.");
            }

            Console.WriteLine(request.UserName);
            Console.WriteLine(request.Date);
            Console.WriteLine(request.Index);

            var query = $@"
            {{
            ""query"": {{
                ""bool"": {{
                    ""filter"": [
                        {{
                            ""bool"": {{
                                ""should"": [
                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{request.UserName}"" }} }}
                                ],
                                ""minimum_should_match"": 1
                            }}
                        }},
                        {{
                            ""range"": {{
                                ""TIMESTAMP"": {{
                                    ""gte"": ""{request.Date}"",
                                    ""lte"": ""now/M""
                                }}
                            }}
                        }}
                    ]
                }}
            }},
            ""aggs"": {{
                ""monthly_data"": {{
                    ""date_histogram"": {{
                        ""field"": ""TIMESTAMP"",
                        ""calendar_interval"": ""month"",
                        ""format"": ""yyyy-MM""
                    }},
                    ""aggs"": {{
                        ""completed_issues_count"": {{
                            ""value_count"": {{
                                ""field"": ""TIMESTAMP""
                            }}
                        }},
                        ""breach_issues_count"": {{
                            ""filters"": {{
                                ""filters"": {{
                                    ""level_2_breach"": {{
                                        ""bool"": {{
                                            ""must"": [
                                                {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                            ]
                                        }}
                                    }},
                                    ""level_3_breach"": {{
                                        ""bool"": {{
                                            ""must"": [
                                                {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                            ]
                                        }}
                                    }},
                                    ""level_4_breach"": {{
                                        ""bool"": {{
                                            ""must"": [
                                                {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                            ]
                                        }}
                                    }},
                                    ""level_5_breach"": {{
                                        ""bool"": {{
                                            ""must"": [
                                                {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                {{ ""term"": {{ ""L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                            ]
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            }},
            ""size"": 0
            }}";

            // Execute Elasticsearch query
                if (string.IsNullOrEmpty(request.Index))
                {
                    return BadRequest("Index must be provided.");
                }

            // Execute Elasticsearch query
            var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, request.Index);
            
            // Initialize the result list
            var monthlyDataList = new List<CompletedAndBreachedApi>();

            // Iterate through the buckets to extract the data
            if (response.TryGetProperty("aggregations", out var aggregations))
            {
                var monthlyDataBuckets = aggregations.GetProperty("monthly_data").GetProperty("buckets");

                foreach (var bucket in monthlyDataBuckets.EnumerateArray())
                {
                    var month = bucket.GetProperty("key_as_string").GetString();
                    var completedIssuesCount = bucket.GetProperty("completed_issues_count").GetProperty("value").GetInt32();

                    var breachIssuesCount = 0;
                    if (bucket.GetProperty("breach_issues_count").TryGetProperty("buckets", out var breachBuckets))
                    {
                        // Sum up breaches from all levels (level_2_breach, level_3_breach, level_4_breach, level_5_breach)
                        breachIssuesCount = breachBuckets
                            .EnumerateObject()
                            .Sum(breach => breach.Value.GetProperty("doc_count").GetInt32());
                    }

                    monthlyDataList.Add(new CompletedAndBreachedApi
                    {
                        Month = month,
                        CompletedIssuesCount = completedIssuesCount,
                        BreachedIssuesCount = breachIssuesCount
                    });
                }
            }

            // Return the extracted data
            return Ok(monthlyDataList);
        }

        // POST api/jira/breached_and_non_breached
        [HttpPost("breached_and_non_breached")]
        [Authorize]
        public async Task<IActionResult> BreachedAndNonBreachedApi([FromBody] SearchByUserDateRequest request)
        {
            // Ensure the 'user_name' and 'date' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("UserName and Date must be provided.");
            }

            Console.WriteLine(request.UserName);
            Console.WriteLine(request.Date);
            Console.WriteLine(request.Index);

            var query = $@"
            {{
            ""query"": {{
                ""bool"": {{
                    ""filter"": [
                        {{
                            ""bool"": {{
                                ""should"": [
                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{request.UserName}"" }} }}
                                ],
                                ""minimum_should_match"": 1
                            }}
                        }},
                        {{
                            ""range"": {{
                                ""TIMESTAMP"": {{
                                    ""gte"": ""{request.Date}"",
                                    ""lte"": ""now/M""
                                }}
                            }}
                        }}
                    ]
                }}
            }},
            ""aggs"": {{
                ""issue_counts"": {{
                    ""filters"": {{
                        ""filters"": {{
                            ""breach_issues"": {{
                                ""bool"": {{
                                    ""should"": [
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                                ]
                                            }}
                                        }}
                                    ],
                                    ""minimum_should_match"": 1
                                }}
                            }},
                            ""non_breach_issues"": {{
                                ""bool"": {{
                                    ""should"": [
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                    {{ ""term"": {{ ""L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                ]
                                            }}
                                        }}
                                    ],
                                    ""minimum_should_match"": 1
                                }}
                            }}
                        }}
                    }}
                }}
            }},
            ""size"": 0
            }}";

            try
                {
                
                // Execute Elasticsearch query
                if (string.IsNullOrEmpty(request.Index))
                {
                    return BadRequest("Index must be provided.");
                }

                // Execute Elasticsearch query
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, request.Index);

                // Safely extract the counts for breached and non-breached issues
                int breachedCount = 0;
                int nonBreachCount = 0;

                if (response.TryGetProperty("aggregations", out var aggregations) &&
                    aggregations.TryGetProperty("issue_counts", out var issueCounts) &&
                    issueCounts.TryGetProperty("buckets", out var buckets))
                {
                    if (buckets.TryGetProperty("breach_issues", out var breachIssues))
                    {
                        breachedCount = breachIssues.TryGetProperty("doc_count", out var breachDocCount) 
                            ? breachDocCount.GetInt32() 
                            : 0;
                    }

                    if (buckets.TryGetProperty("non_breach_issues", out var nonBreachIssues))
                    {
                        nonBreachCount = nonBreachIssues.TryGetProperty("doc_count", out var nonBreachDocCount) 
                            ? nonBreachDocCount.GetInt32() 
                            : 0;
                    }
                }

                // Return the formatted result
                var result = new BreachedAndNonBreachedApiResponse
                {
                    TotalIssuesBreachCount = breachedCount,
                    TotalIssuesNonBreachCount = nonBreachCount
                };

                return Ok(result);
            }
        catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // POST api/jira/completionrate
        [HttpPost("completionrate")]
        [Authorize]
        public async Task<IActionResult> CompletionRateApi([FromBody] SearchByUserDateRequest request)
        {
            // Ensure the 'user_name' and 'date' are provided in the request
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Date))
            {
                return BadRequest("UserName and Date must be provided.");
            }

            Console.WriteLine(request.UserName);
            Console.WriteLine(request.Date);
            Console.WriteLine(request.Index);

            var query = $@"
            {{
                ""size"": 0,
                ""query"": {{
                    ""bool"": {{
                        ""filter"": [
                            {{
                                ""bool"": {{
                                    ""should"": [
                                        {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{request.UserName}"" }} }}
                                    ],
                                    ""minimum_should_match"": 1
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""TIMESTAMP"": {{
                                        ""gte"": ""{request.Date}"",
                                        ""lte"": ""now/M""
                                    }}
                                }}
                            }}
                        ]
                    }}
                }},
                ""aggs"": {{
                    ""monthly_data"": {{
                        ""date_histogram"": {{
                            ""field"": ""TIMESTAMP"",
                            ""calendar_interval"": ""month"",
                            ""format"": ""yyyy-MM""
                        }},
                        ""aggs"": {{
                            ""total_issues_non_breach"": {{
                                ""filters"": {{
                                    ""filters"": {{
                                        ""non_breach_issues"": {{
                                            ""bool"": {{
                                                ""should"": [
                                                    {{
                                                        ""bool"": {{
                                                            ""must"": [
                                                                {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                                {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                            ]
                                                        }}
                                                    }},
                                                    {{
                                                        ""bool"": {{
                                                            ""must"": [
                                                                {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                                {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                            ]
                                                        }}
                                                    }},
                                                    {{
                                                        ""bool"": {{
                                                            ""must"": [
                                                                {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                                {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                            ]
                                                        }}
                                                    }},
                                                    {{
                                                        ""bool"": {{
                                                            ""must"": [
                                                                {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{request.UserName}"" }} }},
                                                                {{ ""term"": {{ ""L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                            ]
                                                        }}
                                                    }}
                                                ],
                                                ""minimum_should_match"": 1
                                            }}
                                        }}
                                    }}
                                }}
                            }},
                            ""total_issues_completed"": {{
                                ""value_count"": {{
                                    ""field"": ""TIMESTAMP""
                                }}
                            }},
                            ""completion_rate"": {{
                                ""bucket_script"": {{
                                    ""buckets_path"": {{
                                        ""non_breach"": ""total_issues_non_breach['non_breach_issues']._count"",
                                        ""completed"": ""total_issues_completed""
                                    }},
                                    ""script"": ""params.completed > 0 ? (params.non_breach / params.completed) * 100 : 0""
                                }}
                            }}
                        }}
                    }}
                }}
            }}";

            try
            {
                // Execute Elasticsearch query
                if (string.IsNullOrEmpty(request.Index))
                {
                    return BadRequest("Index must be provided.");
                }
                
                // Execute Elasticsearch query
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, request.Index);

                Console.WriteLine(response);

                var monthlyData = response
                .GetProperty("aggregations")
                .GetProperty("monthly_data")
                .GetProperty("buckets")
                .EnumerateArray()
                .Select(bucket => new CompletionRateApiResponse
                {
                    Month = bucket.GetProperty("key_as_string").GetString(),
                    TotalIssuesNonBreachCount = bucket.GetProperty("total_issues_non_breach")
                        .GetProperty("buckets")
                        .GetProperty("non_breach_issues")
                        .GetProperty("doc_count").GetInt32(),
                    TotalIssuesCompleted = bucket.GetProperty("total_issues_completed")
                        .GetProperty("value").GetInt32(),
                    CompletionRate = bucket.TryGetProperty("completion_rate", out var rateProperty)
                        ? rateProperty.GetProperty("value").GetDouble()
                        : 0.0,  // Default to 0.0 if completion_rate is missing
                })
                .ToList();


                // Return the formatted monthly data
                return Ok(monthlyData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}