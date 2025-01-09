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

namespace TfsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize]
    public class TfsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ElasticSearchService _elasticSearchService;
        private readonly string _indexName;

        public TfsController(IHttpClientFactory httpClientFactory, ElasticSearchService elasticSearchService, IOptions<IndexesName> settings)
        {
            _httpClientFactory = httpClientFactory;
             _elasticSearchService = elasticSearchService;
             _indexName = settings.Value.TFS;

        }

        [HttpPost]
        [Route("bugs_count")]
        public async Task<IActionResult> BugsCountApi([FromBody] SearchRequest request)
        {

            var username = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var rawEmail = User.Identity?.Name;

            Console.WriteLine("Username: "+username);
            Console.WriteLine("Email: "+rawEmail);
            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;

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
                                                ""CURRENT_ASSIGNEE.keyword"": ""*{email}*""
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
                                                ""ASSIGNED_TO.keyword"": ""*{email}*""
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
                var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

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

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;


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
                                                ""CURRENT_ASSIGNEE.keyword"": ""*{email}*""
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
                                                ""ASSIGNED_TO.keyword"": ""*{email}*""
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
                var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

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

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;

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
                                                ""CURRENT_ASSIGNEE.keyword"": ""*{email}*""
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
                                                ""ASSIGNED_TO.keyword"": ""*{email}*""
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
                var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

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

            var rawEmail = User.Identity?.Name;

            var email = rawEmail?.Contains("#") == true 
                ? rawEmail.Split('#').Last() 
                : rawEmail;

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
                                    ""PARENT_ASSIGNEE.keyword"": ""*{email}*""
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
                var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

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

    [HttpGet]
    [Route("workitems")]
    public async Task<IActionResult> GetWorkItems([FromQuery] int month)
    {
        

        var rawEmail = User.Identity?.Name;

        // var email = rawEmail?.Contains("#") == true 
        //     ? rawEmail.Split('#').Last() 
        //     : rawEmail;
        var email = "hamza01961@gmail.com";

        // Construct the query as a JSON string, replacing email and month dynamically
        string query = $@"
        {{
            ""query"": {{
                ""bool"": {{
                    ""must"": [
                        {{
                            ""wildcard"": {{
                                ""ASSIGNED_TO.keyword"": ""*{email}*""
                            }}
                        }},
                        {{
                            ""term"": {{
                                ""STREAM_NAME.keyword"": ""completed_work_items""
                            }}
                        }},
                        {{
                            ""range"": {{
                                ""CLOSED_DATE"": {{
                                    ""gte"": ""now-{month}M/M"",
                                    ""lte"": ""now""
                                }}
                            }}
                        }},
                        {{
                            ""terms"": {{
                                ""WORK_ITEM_TYPE.keyword"": [""Task""]
                            }}
                        }}
                    ]
                }}
            }}
        }}";

        try{
            // Call the service method to execute the query
            var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

            // Extract hits from the response
            var hits = response.GetProperty("hits").GetProperty("hits");

            // Transform the hits into an array of JSON objects
            var result = hits.EnumerateArray().Select(hit => new
            {
                createdDate = hit.GetProperty("_source").GetProperty("CREATED_DATE").GetString(),
                closedDate = hit.GetProperty("_source").GetProperty("CLOSED_DATE").GetString(),
                title = hit.GetProperty("_source").GetProperty("TITLE").GetString(),
                workItemId= hit.GetProperty("_source").GetProperty("WORK_ITEM_ID").GetInt32(),
                workItemType = hit.GetProperty("_source").GetProperty("WORK_ITEM_TYPE").GetString()
            }).ToArray();

            // Return the result as JSON
            return Ok(result);
        }
        catch (Exception ex){
            // Log the error and return a bad request response
            return BadRequest($"Error querying Elasticsearch: {ex.Message}");
        }
    
    }

    }
}
