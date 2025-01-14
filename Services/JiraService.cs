using System.Text.Json;
using Microsoft.Extensions.Options;
using Jira.Models;
public class JiraService
{
    private readonly ElasticSearchService _elasticSearchService;
    private readonly string _indexName;

    // Constructor to inject ElasticSearchService
    public JiraService(ElasticSearchService elasticSearchService, IOptions<IndexesName> settings)
    {
        _elasticSearchService = elasticSearchService;
        _indexName = settings.Value.JIRA;
    }

    public async Task<List<CompletedAndBreachedApi>> getCompletedAndBreachedIssues(string date, string username)
    {
        var query = getCompletedAndBreachedQuery(date, username);

        try
        {
            // Execute Elasticsearch query
            var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

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

            return monthlyDataList;
        }

        catch (Exception ex)
            {
                Console.WriteLine( $"Internal server error: {ex.Message}");
                return new List<CompletedAndBreachedApi>(); // Return empty array in case of error
            }
    }

    public async Task<JsonElement> getJiraInsights(string Issue_key, string displayName)
    {
        try{
            var getjiraissueinfoquery = getJiraIssueInfoQuery(Issue_key);

            var getjiraissueinfo = await _elasticSearchService.ExecuteElasticsearchQueryAsync(getjiraissueinfoquery, _indexName);

            var total_1 = getjiraissueinfo.GetProperty("hits") .GetProperty("total") .GetProperty("value").GetInt32();

             if(total_1 > 0){
                var filteredResponse = FilterResponse(getjiraissueinfo, displayName);

                // Add TaskType key with hardcoded value "JIRA"
                var filteredResponseDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(filteredResponse.GetRawText());
                filteredResponseDict["TASKTYPE"] = JsonDocument.Parse("\"JIRA\"").RootElement;

                return JsonSerializer.SerializeToElement(filteredResponseDict);
             }
             else{
                var json = JsonDocument.Parse("{\"message\": \"No issue found\"}");
                return json.RootElement;
             }
        }
        catch (Exception ex)
        {
            Console.WriteLine( $"Internal server error: {ex.Message}");
            return new JsonElement{}; // Return empty array in case of error
        }
    }

    public async Task<BreachedAndNonBreachedApiResponse> getBreachedAndNonBreachedIssues(string date, string username)
    {
        var query = getBreachedAndNonBreachedIssuesQuery(date, username);

        try
        {
        // Execute Elasticsearch query
        var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

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

        return result;
        }

    catch (Exception ex)
        {
            Console.WriteLine( $"Internal server error: {ex.Message}");
            return new BreachedAndNonBreachedApiResponse{}; // Return empty array in case of error
        }
    }

    public string getJiraIssueInfoQuery(string Issue_key)
    {
        var query = $@"
        {{
            ""size"": 1,
            ""_source"": [
                ""CURRENT_STATUS"",
                ""SUMMARY"",
                ""PRIORITY_NAME"",
                ""TIMESTAMP"",
                ""ISSUE_KEY"",
                ""SEVERITYWISECATEGORY_VALUE"",
                ""LEVEL_2_ASSIGNEE"",
                ""LEVEL_3_ASSIGNEE"",
                ""LEVEL_4_ASSIGNEE"",
                ""LEVEL_5_ASSIGNEE"",
                ""L2_WORKING_ONGOINGCYCLE_BREACHED"",
                ""L2_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS"",
                ""L2_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS"",
                ""L2_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS"",
                ""L3_WORKING_ONGOINGCYCLE_BREACHED"",
                ""L3_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS"",
                ""L3_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS"",
                ""L3_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS"",
                ""L4_WORKING_ONGOINGCYCLE_BREACHED"",
                ""L4_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS"",
                ""L4_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS"",
                ""L4_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS"",
                ""L5_WORKING_ONGOINGCYCLE_BREACHED"",
                ""L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_GOALDURATION_MILLIS"",
                ""L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_ELAPSEDTIME_MILLIS"",
                ""L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_REMAININGTIME_MILLIS""
            ],
            ""query"": {{
                ""term"": {{
                    ""ISSUE_KEY.keyword"": ""{Issue_key}""
                }}
            }}
        }}";



        return query;
    }

    public static JsonElement FilterResponse(JsonElement jsonResponse, string displayName)
    {
        try
        {
            var hits = jsonResponse.GetProperty("hits").GetProperty("hits");

            // Create a dictionary to store filtered results
            var filteredResults = new Dictionary<string, JsonElement>();

            if (hits.GetArrayLength() > 0)
            {
                var source = hits[0].GetProperty("_source");

                // Levels to check
                var levels = new[] { 
                    new { Level = "LEVEL_2", Prefix = "L2" }, 
                    new { Level = "LEVEL_3", Prefix = "L3" }, 
                    new { Level = "LEVEL_4", Prefix = "L4" }, 
                    new { Level = "LEVEL_5", Prefix = "L5" }
                };

                foreach (var level in levels)
                {
                    if (source.TryGetProperty(level.Level + "_ASSIGNEE", out var assignee) && assignee.GetString() == displayName)
                    {
                        AddRelevantMetrics(source, filteredResults, level.Prefix);
                        break;
                    }
                }

                AddCommonFields(source, filteredResults);
            }

            return JsonSerializer.SerializeToElement(filteredResults);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing response: {ex.Message}");
            return JsonDocument.Parse("{}").RootElement;
        }
    }

    private static void AddRelevantMetrics(JsonElement source, Dictionary<string, JsonElement> filteredResults, string prefix)
    {
        var fields = new[] { "REMAININGTIME_MILLIS", "ELAPSEDTIME_MILLIS", "GOALDURATION_MILLIS", "BREACHED" };

        foreach (var field in fields)
        {
            if (source.TryGetProperty($"{prefix}_WORKING_ONGOINGCYCLE_{field}", out var value))
            {
                filteredResults[$"WORKING_ONGOINGCYCLE_{field}"] = value;
            }
        }
    }

    private static void AddCommonFields(JsonElement source, Dictionary<string, JsonElement> filteredResults)
    {
        var commonFields = new[] { "CURRENT_STATUS", "SUMMARY", "ISSUE_KEY", "PRIORITY_NAME", "TIMESTAMP", "SEVERITYWISECATEGORY_VALUE" };

        foreach (var field in commonFields)
        {
            if (source.TryGetProperty(field, out var fieldValue))
            {
                filteredResults[field] = fieldValue;
            }
        }
    }

    private static string getCompletedAndBreachedQuery(string date, string username)
    {
        var query = $@"
            {{
            ""query"": {{
                ""bool"": {{
                    ""filter"": [
                        {{
                            ""bool"": {{
                                ""should"": [
                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }}
                                ],
                                ""minimum_should_match"": 1
                            }}
                        }},
                        {{
                            ""range"": {{
                                ""TIMESTAMP"": {{
                                    ""gte"": ""{date}"",
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
                                                {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                            ]
                                        }}
                                    }},
                                    ""level_3_breach"": {{
                                        ""bool"": {{
                                            ""must"": [
                                                {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                            ]
                                        }}
                                    }},
                                    ""level_4_breach"": {{
                                        ""bool"": {{
                                            ""must"": [
                                                {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                            ]
                                        }}
                                    }},
                                    ""level_5_breach"": {{
                                        ""bool"": {{
                                            ""must"": [
                                                {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }},
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

        return query;
    }

    private static string getBreachedAndNonBreachedIssuesQuery(string date, string username)
    {
        var query = $@"
            {{
            ""query"": {{
                ""bool"": {{
                    ""filter"": [
                        {{
                            ""bool"": {{
                                ""should"": [
                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }}
                                ],
                                ""minimum_should_match"": 1
                            }}
                        }},
                        {{
                            ""range"": {{
                                ""TIMESTAMP"": {{
                                    ""gte"": ""{date}"",
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
                                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                    {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                    {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                    {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": true }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }},
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
                                                    {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                    {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                    {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                    {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                ]
                                            }}
                                        }},
                                        {{
                                            ""bool"": {{
                                                ""must"": [
                                                    {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }},
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

        return query;
    }

    public async Task<List<CompletionRateApiResponse>> getCompletionRate(string date, string username)
    {
        var query = getCompletionRateQuery(date, username);

        try
        {
            // Execute Elasticsearch query
            var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

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

            return monthlyData;

        }
        catch(Exception ex)
        {
            Console.WriteLine( $"Internal server error: {ex.Message}");
            return new List<CompletionRateApiResponse>();
        }
    }

    private static string getCompletionRateQuery(string date, string username)
    {
        var query = $@"
            {{
                ""size"": 0,
                ""query"": {{
                    ""bool"": {{
                        ""filter"": [
                            {{
                                ""bool"": {{
                                    ""should"": [
                                        {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }}
                                    ],
                                    ""minimum_should_match"": 1
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""TIMESTAMP"": {{
                                        ""gte"": ""{date}"",
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
                                                                {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                                {{ ""term"": {{ ""L2_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                            ]
                                                        }}
                                                    }},
                                                    {{
                                                        ""bool"": {{
                                                            ""must"": [
                                                                {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                                {{ ""term"": {{ ""L3_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                            ]
                                                        }}
                                                    }},
                                                    {{
                                                        ""bool"": {{
                                                            ""must"": [
                                                                {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                                                {{ ""term"": {{ ""L4_WORKING_ONGOINGCYCLE_BREACHED"": false }} }}
                                                            ]
                                                        }}
                                                    }},
                                                    {{
                                                        ""bool"": {{
                                                            ""must"": [
                                                                {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }},
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

        return query;
    }

    public async Task<List<GetCompletedIssueListApiResponse>> getCompletedIssueList(int month, string username)
    {
        var query = getCompletedIssuesListQuery(month, username);

        try
        {
            var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

            var IssueList = response
                .GetProperty("hits")
                .GetProperty("hits")
                .EnumerateArray()
                .Select(hits => new GetCompletedIssueListApiResponse
                {
                    Id = hits.GetProperty("_id").GetString(),
                    IssueKey = hits.GetProperty("_source") .GetProperty("ISSUE_KEY").GetString(),
                    Date = hits.GetProperty("_source") .GetProperty("TIMESTAMP").GetString(),
                    Status = hits.GetProperty("_source") .GetProperty("CURRENT_STATUS").GetString(),
                    Summary = hits.GetProperty("_source") .GetProperty("SUMMARY").GetString(),
                    Severity = hits.GetProperty("_source") .GetProperty("SEVERITYWISECATEGORY_VALUE").GetString(),
                    L2_breach = hits.GetProperty("_source").GetProperty("L2_WORKING_ONGOINGCYCLE_BREACHED").ToString(),
                    L3_breach = hits.GetProperty("_source").GetProperty("L3_WORKING_ONGOINGCYCLE_BREACHED").ToString(),
                    L4_breach = hits.GetProperty("_source").GetProperty("L4_WORKING_ONGOINGCYCLE_BREACHED").ToString(),
                    L5_breach = hits.GetProperty("_source").GetProperty("L5_DEVOPS_QA_WORKING_ONGOINGCYCLE_BREACHED").ToString()
                })
                .ToList();

            return IssueList;
        }
        catch (Exception ex)
        {
            Console.WriteLine( $"Internal server error: {ex.Message}");
            return new List<GetCompletedIssueListApiResponse>(); // Return empty array in case of error
        }


    }
    private static string getCompletedIssuesListQuery(int month, string username)
    {
        var query = $@"
            {{
                ""size"": 1,
                ""query"": {{
                    ""bool"": {{
                        ""filter"": [
                            {{
                                ""bool"": {{
                                    ""should"": [
                                        {{ ""term"": {{ ""LEVEL_2_ASSIGNEE.keyword"": ""{username}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_3_ASSIGNEE.keyword"": ""{username}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_4_ASSIGNEE.keyword"": ""{username}"" }} }},
                                        {{ ""term"": {{ ""LEVEL_5_ASSIGNEE.keyword"": ""{username}"" }} }}
                                    ],
                                    ""minimum_should_match"": 1
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""TIMESTAMP"": {{
                                        ""gte"": ""now-{month}M/M"",
                                        ""lte"": ""now""
                                    }}
                                }}
                            }}
                        ]
                    }}
                }}
            }}";

        return query;
    }
}

