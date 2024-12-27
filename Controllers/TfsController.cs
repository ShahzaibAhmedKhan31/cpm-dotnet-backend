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

        public TfsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

            // Serialize the JSON string into HttpContent
            var content = new StringContent(query, Encoding.UTF8, "application/json");

            // Make HTTP request to Elasticsearch
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("http://172.174.172.29:9200/tfs_index/_search?pretty", content);

            if (!response.IsSuccessStatusCode)
            {
                // Log the response body for debugging
                var resp = await response.Content.ReadAsStringAsync();
                return BadRequest($"Error querying Elasticsearch. Response: {resp}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonConvert.DeserializeObject<SearchResponse>(responseBody);

            // Return Elasticsearch response
            var buckets = searchResponse.Aggregations?.Months?.Buckets;
            return Ok(buckets);
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
            var content = new StringContent(query, Encoding.UTF8, "application/json");

            // Make HTTP request to Elasticsearch
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("http://172.174.172.29:9200/tfs_index/_search?pretty", content);

            if (!response.IsSuccessStatusCode)
            {
                // Log the response body for debugging
                var resp = await response.Content.ReadAsStringAsync();
                return BadRequest($"Error querying Elasticsearch. Response: {resp}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonConvert.DeserializeObject<TaskResponse.Models.TaskResponse>(responseBody);

            // Return Elasticsearch response
            var buckets = searchResponse.Aggregations?.Months?.Buckets;
            return Ok(buckets);
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


            // Serialize the JSON string into HttpContent
            var content = new StringContent(query, Encoding.UTF8, "application/json");

            // Make HTTP request to Elasticsearch
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("http://172.174.172.29:9200/tfs_index/_search?pretty", content);

            if (!response.IsSuccessStatusCode)
            {
                // Log the response body for debugging
                var resp = await response.Content.ReadAsStringAsync();
                return BadRequest($"Error querying Elasticsearch. Response: {resp}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonConvert.DeserializeObject<TaskCompletionResponse.Models.TaskCompletionResponse>(responseBody);

            // Return Elasticsearch response
            var buckets = searchResponse.Aggregations?.Months?.Buckets;
            return Ok(buckets);
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



            // Serialize the JSON string into HttpContent
            var content = new StringContent(query, Encoding.UTF8, "application/json");

            // Make HTTP request to Elasticsearch
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("http://172.174.172.29:9200/tfs_index/_search?pretty", content);

            if (!response.IsSuccessStatusCode)
            {
                // Log the response body for debugging
                var resp = await response.Content.ReadAsStringAsync();
                return BadRequest($"Error querying Elasticsearch. Response: {resp}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonConvert.DeserializeObject<WorkItemBugCountResponse.Models.WorkItemBugCountResponse>(responseBody);

            
            var result = new List<object>();

            foreach (var item in searchResponse.Aggregations.WorkitemBugsCount.Buckets)
            {
                var workItem = new
                {
                    workItemId = item.Key,
                    bugsCount = item.Bugs.DocCount
                };
                result.Add(workItem);
            }

            return Ok(result);
        }

    }
}
