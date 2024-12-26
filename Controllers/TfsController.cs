// Controllers/ElasticSearchController.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TfsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TfsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TfsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
        {
            // Calculate the date range
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-request.Months);

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

    }
}
