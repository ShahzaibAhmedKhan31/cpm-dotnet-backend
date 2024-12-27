// Controllers/ElasticSearchController.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ApiRequest.Models;
using Microsoft.AspNetCore.Authorization;

namespace TfsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TfsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ElasticSearchService _elasticSearchService;

        public TfsController(IHttpClientFactory httpClientFactory, ElasticSearchService elasticSearchService)
        {
            _httpClientFactory = httpClientFactory;
             _elasticSearchService = elasticSearchService;
        }

        [HttpPost]
        [Route("bugs_count")]
        public async Task<IActionResult> BugsCountApi([FromBody] SearchRequest request)
        {
            // Construct the query as a JSON string
            string query = $@"
            {{
                ""size"": 0,
                ""query"": {{
                    ""bool"": {{
                        ""should"": [
                            {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""STREAM_NAME.keyword"": ""created_work_items""
                                            }}
                                        }},
                                        {{
                                            ""wildcard"": {{
                                                ""CURRENT_ASSIGNEE.keyword"": ""*{request.Email}*""
                                            }}
                                        }}
                                    ]
                                }}
                            }},
                            {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""STREAM_NAME.keyword"": ""completed_work_items""
                                            }}
                                        }},
                                        {{
                                            ""wildcard"": {{
                                                ""ASSIGNED_TO.keyword"": ""*{request.Email}*""
                                            }}
                                        }}
                                    ]
                                }}
                            }}
                        ],
                        ""minimum_should_match"": 1,
                        ""filter"": [
                            {{
                                ""term"": {{
                                    ""WORK_ITEM_TYPE.keyword"": ""Bug""
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""CREATED_DATE"": {{
                                        ""gte"": ""now-{request.Months}M/M"",
                                        ""lte"": ""now/M""
                                    }}
                                }}
                            }}
                        ]
                    }}
                }},
                ""aggs"": {{
                    ""months"": {{
                        ""date_histogram"": {{
                            ""field"": ""CREATED_DATE"",
                            ""calendar_interval"": ""month"",
                            ""format"": ""yyyy-MM""
                        }},
                        ""aggs"": {{
                            ""bugs_assigned"": {{
                                ""filter"": {{
                                    ""term"": {{
                                        ""STREAM_NAME.keyword"": ""created_work_items""
                                    }}
                                }}
                            }},
                            ""bugs_completed"": {{
                                ""filter"": {{
                                    ""term"": {{
                                        ""STREAM_NAME.keyword"": ""completed_work_items""
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            }}";


            try
            {

                // Use the service to execute the Elasticsearch query
                var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, "tfs_index");

                // Parse the response to access "aggregations" -> "months" -> "buckets"
                var aggregations = searchResponse.GetProperty("aggregations");
                var months = aggregations.GetProperty("months");
                var buckets = months.GetProperty("buckets");
                
                // Use the service to execute the Elasticsearch query
                var result = buckets.EnumerateArray().Select(bucket => new
                {
                    date = bucket.GetProperty("key_as_string").GetString(),
                    bugsAssigned = new
                    {
                        docCount = bucket.GetProperty("bugs_assigned").GetProperty("doc_count").GetInt32()
                    },
                    bugsCompleted = new
                    {
                        docCount = bucket.GetProperty("bugs_completed").GetProperty("doc_count").GetInt32()
                    }
                });

                // Return the processed buckets
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("task_count")]
        public async Task<IActionResult> TaskCount([FromBody] SearchRequest request)
        {

            // Construct the query as a JSON string
            string query = $@"
            {{
                ""size"": 0,
                ""query"": {{
                    ""bool"": {{
                        ""should"": [
                            {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""STREAM_NAME.keyword"": ""created_work_items""
                                            }}
                                        }},
                                        {{
                                            ""wildcard"": {{
                                                ""CURRENT_ASSIGNEE.keyword"": ""*{request.Email}*""
                                            }}
                                        }}
                                    ]
                                }}
                            }},
                            {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""STREAM_NAME.keyword"": ""completed_work_items""
                                            }}
                                        }},
                                        {{
                                            ""wildcard"": {{
                                                ""ASSIGNED_TO.keyword"": ""*{request.Email}*""
                                            }}
                                        }}
                                    ]
                                }}
                            }}
                        ],
                        ""minimum_should_match"": 1,
                        ""filter"": [
                            {{
                                ""term"": {{
                                    ""WORK_ITEM_TYPE.keyword"": ""Task""
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""CREATED_DATE"": {{
                                        ""gte"": ""now-{request.Months}M/M"",
                                        ""lte"": ""now/M""
                                    }}
                                }}
                            }}
                        ]
                    }}
                }},
                ""aggs"": {{
                    ""months"": {{
                        ""date_histogram"": {{
                            ""field"": ""CREATED_DATE"",
                            ""calendar_interval"": ""month"",
                            ""format"": ""yyyy-MM""
                        }},
                        ""aggs"": {{
                            ""tasks_assigned"": {{
                                ""filter"": {{
                                    ""term"": {{
                                        ""STREAM_NAME.keyword"": ""created_work_items""
                                    }}
                                }}
                            }},
                            ""tasks_completed"": {{
                                ""filter"": {{
                                    ""term"": {{
                                        ""STREAM_NAME.keyword"": ""completed_work_items""
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            }}";


            // Serialize the JSON string into HttpContent
            // var content = new StringContent(query, Encoding.UTF8, "application/json");

            // // Make HTTP request to Elasticsearch
            try
            {

                // Use the service to execute the Elasticsearch query
                var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, "tfs_index");

                // Parse the response to access "aggregations" -> "months" -> "buckets"
                var aggregations = searchResponse.GetProperty("aggregations");
                var months = aggregations.GetProperty("months");
                var buckets = months.GetProperty("buckets");
                
                // Use the service to execute the Elasticsearch query
                var result = buckets.EnumerateArray().Select(bucket => new
                {
                    date = bucket.GetProperty("key_as_string").GetString(),
                    tasksAssigned = new
                    {
                        docCount = bucket.GetProperty("tasks_assigned").GetProperty("doc_count").GetInt32()
                    },
                    tasksCompleted = new
                    {
                        docCount = bucket.GetProperty("tasks_completed").GetProperty("doc_count").GetInt32()
                    }
                });

                // Return the processed buckets
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }



        }

        [HttpPost]
        [Route("task_completion_rate")]
        public async Task<IActionResult> TaskCompletionRate([FromBody] SearchRequest request)
        {

            // Construct the query as a JSON string
            string query = $@"
            {{
                ""size"": 0,
                ""query"": {{
                    ""bool"": {{
                        ""should"": [
                            {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""STREAM_NAME.keyword"": ""created_work_items""
                                            }}
                                        }},
                                        {{
                                            ""wildcard"": {{
                                                ""CURRENT_ASSIGNEE.keyword"": ""*{request.Email}*""
                                            }}
                                        }}
                                    ]
                                }}
                            }},
                            {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""STREAM_NAME.keyword"": ""completed_work_items""
                                            }}
                                        }},
                                        {{
                                            ""wildcard"": {{
                                                ""ASSIGNED_TO.keyword"": ""*{request.Email}*""
                                            }}
                                        }}
                                    ]
                                }}
                            }}
                        ],
                        ""minimum_should_match"": 1,
                        ""filter"": [
                            {{
                                ""term"": {{
                                    ""WORK_ITEM_TYPE.keyword"": ""Task""
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""CREATED_DATE"": {{
                                        ""gte"": ""now-{request.Months}M/M"",
                                        ""lte"": ""now/M""
                                    }}
                                }}
                            }}
                        ]
                    }}
                }},
                ""aggs"": {{
                    ""months"": {{
                        ""date_histogram"": {{
                            ""field"": ""CREATED_DATE"",
                            ""calendar_interval"": ""month"",
                            ""format"": ""yyyy-MM""
                        }},
                        ""aggs"": {{
                            ""tasks_assigned"": {{
                                ""filter"": {{
                                    ""term"": {{
                                        ""STREAM_NAME.keyword"": ""created_work_items""
                                    }}
                                }}
                            }},
                            ""tasks_completed"": {{
                                ""filter"": {{
                                    ""term"": {{
                                        ""STREAM_NAME.keyword"": ""completed_work_items""
                                    }}
                                }}
                            }},
                            ""completion_rate"": {{
                            ""bucket_script"": {{
                                ""buckets_path"": {{
                                ""assigned"": ""tasks_assigned._count"",
                                ""completed"": ""tasks_completed._count""
                                }},
                                ""script"": ""params.assigned > 0 ? (params.completed / params.assigned) * 100 : 0""
                            }}
                            }}
                        }}
                    }}
                }}
            }}";


            try
            {
                // Use the service to execute the Elasticsearch query
                var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, "tfs_index");

                // Process the response (example: accessing aggregations and buckets)
                var aggregations = searchResponse.GetProperty("aggregations");
                var months = aggregations.GetProperty("months");
                var buckets = months.GetProperty("buckets");

                // Transform the buckets into the desired format
                var result = buckets.EnumerateArray().Select(bucket => new
                {
                    date = bucket.GetProperty("key_as_string").GetString(),
                    tasksAssigned = new
                    {
                        docCount = bucket.GetProperty("tasks_assigned").GetProperty("doc_count").GetInt32()
                    },
                    tasksCompleted = new
                    {
                        docCount = bucket.GetProperty("tasks_completed").GetProperty("doc_count").GetInt32()
                    },
                    completionRate = new
                    {
                        value = bucket.GetProperty("completion_rate").GetProperty("value").GetDouble()
                    }
                });

                // Return the transformed result
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Handle exceptions from the service
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("workitem_bug_count")]
        public async Task<IActionResult> WorkItemBugCount([FromBody] SearchRequest request)
        {

            // Construct the query as a JSON string
            string query = $@"
            {{
                ""size"": 0,
                ""query"": {{
                    ""bool"": {{
                        ""must"": [
                            {{
                                ""term"": {{
                                    ""STREAM_NAME.keyword"": ""created_work_items""
                                }}
                            }},
                            {{
                                ""wildcard"": {{
                                    ""PARENT_ASSIGNEE.keyword"": ""*{request.Email}*""
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""CREATED_DATE"": {{
                                        ""gte"": ""now-{request.Months}M/M"",
                                        ""lte"": ""now/M""
                                    }}
                                }}
                            }}
                        ]
                    }}
                }},
                ""aggs"": {{
                    ""workitem_bugs_count"": {{
                        ""terms"": {{
                            ""field"": ""PARENT_WORKITEMID"",
                            ""size"": 10000
                        }},
                        ""aggs"": {{
                            ""bugs"": {{
                                ""filter"": {{
                                    ""term"": {{
                                        ""WORK_ITEM_TYPE.keyword"": ""Bug""
                                    }}
                                }},
                                ""aggs"": {{
                                    ""parent_assignee"": {{
                                        ""top_hits"": {{
                                            ""_source"": {{
                                                ""includes"": [""PARENT_ASSIGNEE""]
                                            }},
                                            ""size"": 1
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            }}";

            try
            {
                // Call the service method to execute the query
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, "tfs_index");

                var aggregations = response.GetProperty("aggregations");
                var bugsCount = aggregations.GetProperty("workitem_bugs_count");
                var buckets = bugsCount.GetProperty("buckets");

                // Transform the buckets into the desired output format
                var result = buckets.EnumerateArray().Select(bucket => new
                {
                    workItemId = bucket.GetProperty("key").GetInt32(),
                    bugsCount = bucket.GetProperty("bugs").GetProperty("doc_count").GetInt32()
                });

                // Return the transformed result
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log and return the error response
                return BadRequest($"Error querying Elasticsearch: {ex.Message}");
            }

        }

    }
}
