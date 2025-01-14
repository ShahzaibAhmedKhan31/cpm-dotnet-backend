using System.Text.Json;
using Microsoft.Extensions.Options;
using TaskInsightsModel.Models;
public class TfsService
{
    private readonly ElasticSearchService _elasticSearchService;
    private readonly string _indexName;
    private readonly PrService _prService;

    // Constructor to inject ElasticSearchService
    public TfsService(ElasticSearchService elasticSearchService, IOptions<IndexesName> settings, PrService prService)
    {
        _elasticSearchService = elasticSearchService;
        _indexName = settings.Value.TFS;
        _prService = prService;
    }

    public async Task<IEnumerable<object>> getBugsCount(string email, int month)
    {
        var query = getBugsCountQuery(email, month);

        try
        {
            // Use the service to execute the Elasticsearch query
            var response = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

            // Parse the response to access "aggregations" -> "months" -> "buckets"
            var aggregations = response.GetProperty("aggregations");
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

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return Enumerable.Empty<object>();
        }
    }

    public async Task<IEnumerable<object>> getTaskCount(string email, int month)
    {
        var query = getTaskCountQuery(email, month);

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
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return Enumerable.Empty<object>();
        }
    }
    public async Task<IEnumerable<object>> GetWorkItemCountByEmail(List<string> emails, int month, string type)
    {
        // Generate the Elasticsearch query
        var query = getEmailTaskCountQuery(emails, month, type);
        Console.WriteLine(query);

        try
        {
            // Execute the Elasticsearch query
            var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

            // Parse the response to access "aggregations" -> "created_work_items" -> "buckets"
            var aggregations = searchResponse.GetProperty("aggregations");
            var createdWorkItems = aggregations.GetProperty("created_work_items");
            var createdBuckets = createdWorkItems.GetProperty("buckets");

            var completedWorkItems = aggregations.GetProperty("completed_work_items");
            var completedBuckets = completedWorkItems.GetProperty("buckets");

            // Create a list to hold the result
            var result = new List<object>();

            // Iterate through the created work items and match them with the completed work items
            foreach (var createdBucket in createdBuckets.EnumerateArray())
            {
                // Extract the email and created count from the created bucket
                var email = createdBucket.GetProperty("key").GetString();
                var createdCount = createdBucket.GetProperty("doc_count").GetInt32();

                // Find the corresponding completed work item for the email
                var completedBucket = completedBuckets.EnumerateArray().FirstOrDefault(b => b.GetProperty("key").GetString() == email);
                var completedCount = completedBucket.ValueKind != JsonValueKind.Undefined ? completedBucket.GetProperty("doc_count").GetInt32() : 0;

                // Add the email and counts to the result list
                result.Add(new
                {
                    email,
                    createdCount,
                    completedCount
                });
            }

            // Return the processed result
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return Enumerable.Empty<object>();
        }
    }
  
    public async Task<IEnumerable<object>> GetWorkItemCompletionRateByEmail(List<string> emails, int months)
    {
        // Generate the query using the updated method
        var query = getCompeletionRateByEmailQuery(emails, months);

        try
        {
            // Execute the Elasticsearch query
            var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);

            // Access and parse the response aggregations
            var aggregations = searchResponse.GetProperty("aggregations");
            var monthBuckets = aggregations.GetProperty("month_bucket").GetProperty("buckets");

            // Transform the month buckets into the desired result format
            var result = monthBuckets.EnumerateArray().Select(monthBucket => new
            {
                month = monthBucket.GetProperty("key_as_string").GetString(),
                emails = monthBucket.GetProperty("emails").GetProperty("buckets").EnumerateArray().Select(emailBucket => new
                {
                    email = emailBucket.GetProperty("key").GetString(),
                    tasksAssigned = emailBucket.GetProperty("tasks_assigned").GetProperty("doc_count").GetInt32(),
                    tasksCompleted = emailBucket.GetProperty("tasks_completed").GetProperty("doc_count").GetInt32(),
                    completionRate = emailBucket.GetProperty("completion_rate").GetProperty("value").GetDouble()
                })
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return Enumerable.Empty<object>();
        }
    }
    public async Task<IEnumerable<object>> getTaskCompletionRate(string email, int month)
    {
        var query = getTaskCompletionRateQuery(email, month);

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

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return Enumerable.Empty<object>();
        }
    }

    public async Task<object> getWorkItemBugCount(string email, int month)
    {
        var query = getWorkItemBugCountQuery(email, month);

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
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return Enumerable.Empty<object>();
        }
    }

    public async Task<IEnumerable<object>> getWorkItems(string email, int month)
    {
        var query = getWorkItemsQuery(email, month);

        try
        {
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
                workItemId = hit.GetProperty("_source").GetProperty("WORK_ITEM_ID").GetInt32(),
                workItemType = hit.GetProperty("_source").GetProperty("WORK_ITEM_TYPE").GetString()
            }).ToArray();

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return Enumerable.Empty<object>();
        }
    }

    private static string getWorkItemsQuery(string email, int month)
    {
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

        return query;
    }
    private static string getWorkItemBugCountQuery(string email, int month)
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
                                    ""PARENT_ASSIGNEE.keyword"": ""*{email}*""
                                }}
                            }},
                            {{
                                ""range"": {{
                                    ""CREATED_DATE"": {{
                                        ""gte"": ""now-{month}M/M"",
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

        return query;
    }
    private static string getTaskCompletionRateQuery(string email, int month)
    {
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
                                        ""gte"": ""now-{month}M/M"",
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
        return query;
    }
    private static string getTaskCountQuery(string email, int month)
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
                                        ""gte"": ""now-{month}M/M"",
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

        return query;
    }

    private static string getEmailTaskCountQuery(List<string> emails, int month, string type)
    {
        // Construct the "wildcard" conditions for multiple emails dynamically
        var currentAssigneeEmailConditions = string.Join(",", emails.Select(email => $@"
        {{
            ""wildcard"": {{
                ""CURRENT_ASSIGNEE.keyword"": ""*{email}*""
            }}
        }}"));

        var assigneeToEmailConditions = string.Join(",", emails.Select(email => $@"
        {{
            ""wildcard"": {{
                ""ASSIGNED_TO.keyword"": ""*{email}*""
            }}
        }}"));

        // Construct the query as a JSON string
        string query = $@"
    {{
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
                                    ""bool"": {{
                                        ""should"": [
                                            {currentAssigneeEmailConditions}
                                        ]
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
                                    ""bool"": {{
                                        ""should"": [
                                            {assigneeToEmailConditions}
                                        ]
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
                            ""WORK_ITEM_TYPE.keyword"": ""{type}""
                        }}
                    }},
                    {{
                        ""range"": {{
                            ""CREATED_DATE"": {{
                                ""gte"": ""now-{month}M/M"",
                                ""lte"": ""now/M""
                            }}
                        }}
                    }}
                ]
            }}
        }},
        ""aggs"": {{
            ""created_work_items"": {{
                ""terms"": {{
                    ""field"": ""CURRENT_ASSIGNEE.keyword"",
                    ""size"": 1000
                }},
                ""aggs"": {{
                    ""count_created"": {{
                        ""filter"": {{
                            ""term"": {{
                                ""STREAM_NAME.keyword"": ""created_work_items""
                            }}
                        }}
                    }}
                }}
            }},
            ""completed_work_items"": {{
                ""terms"": {{
                    ""field"": ""ASSIGNED_TO.keyword"",
                    ""size"": 1000
                }},
                ""aggs"": {{
                    ""count_completed"": {{
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

        return query;
    }

    private static string getCompeletionRateByEmailQuery(List<string> emails, int months)
    {
        // Construct the "wildcard" conditions for multiple emails dynamically
        var currentAssigneeEmailConditions = string.Join(",", emails.Select(email => $@"
    {{
        ""wildcard"": {{
            ""CURRENT_ASSIGNEE.keyword"": ""*{email}*""
        }}
    }}"));

        var assigneeToEmailConditions = string.Join(",", emails.Select(email => $@"
    {{
        ""wildcard"": {{
            ""ASSIGNED_TO.keyword"": ""*{email}*""
        }}
    }}"));

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
                                    ""bool"": {{
                                        ""should"": [
                                            {currentAssigneeEmailConditions}
                                        ]
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
                                    ""bool"": {{
                                        ""should"": [
                                            {assigneeToEmailConditions}
                                        ]
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
                                ""gte"": ""now-{months}M/M"",
                                ""lte"": ""now/M""
                            }}
                        }}
                    }}
                ]
            }}
        }},
        ""aggs"": {{
            ""month_bucket"": {{
                ""date_histogram"": {{
                    ""field"": ""CREATED_DATE"",
                    ""calendar_interval"": ""month"",
                    ""format"": ""yyyy-MM""
                }},
                ""aggs"": {{
                    ""emails"": {{
                        ""terms"": {{
                            ""script"": {{
                                ""source"": ""if (doc['STREAM_NAME.keyword'].value == 'created_work_items') {{ return doc['CURRENT_ASSIGNEE.keyword'].value }} else {{ return doc['ASSIGNED_TO.keyword'].value }}""
                            }},
                            ""size"": 10
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
            }}
        }}
    }}";

        return query;
    }

    private static string getBugsCountQuery(string email, int month)
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
                                        ""gte"": ""now-{month}M/M"",
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

        return query;
    }

    public async Task<TaskInsights> TfsInsights(int workitem_id)
    {
        string severityQuery = getSeverityQuery(workitem_id);

        string taskQuery = getTaskQuery(workitem_id);

        try
        {
            // Query for severity aggregation
            var severityResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(severityQuery, _indexName);

            var severityCountList = getSeverityCountList(severityResponse);

            // Query for task details
            var taskResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(taskQuery, _indexName);

            var taskDetails = gettaskDetails(taskResponse);

            var getPrInsights = await _prService.GetPrInsights(workitem_id);

            var result = new TaskInsights
            {
                TaskId = taskDetails.TaskId,
                BugsCount = severityCountList,
                CreatedDate = taskDetails.CreatedDate,
                EndDate = taskDetails.EndDate,
                OriginalEstimate = taskDetails.OriginalEstimate,
                InProgressDate = taskDetails.InProgressDate,
                PrCreatedByEmail = getPrInsights.CreatedByEmail,
                PrCreatedDate = getPrInsights.CreatedDate,
                PrCreatedByName = getPrInsights.CreatedByName,
                PrId = getPrInsights.PrId,
                PrTitle = getPrInsights.Title,
                PrTotalNumberOfComments = getPrInsights.TotalNumberOfComments,
                PrLastMergeCommitId = getPrInsights.LastMergeCommitId,
                PrClosedDate = getPrInsights.PrClosedDate,
                PrClosedByName = getPrInsights.PrClosedByName,
                PrFirstCommentDate = getPrInsights.PrFirstCommentDate,
                PrStatus = getPrInsights.PrStatus,
                TaskType = "TFS"
            };

            return result;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return new TaskInsights { }; // Return empty array in case of error
        }
    }

    private string getSeverityQuery(int workItem_id)
    {
        string severityQuery = $@"
            {{
                ""size"": 0,
                ""query"": {{
                    ""bool"": {{
                        ""must"": [
                            {{
                                ""term"": {{
                                    ""PARENT_WORKITEMID"": {workItem_id}
                                }}
                            }},
                            {{
                                ""term"": {{
                                    ""WORK_ITEM_TYPE.keyword"": ""Bug""
                                }}
                            }}
                        ]
                    }}
                }},
                ""aggs"": {{
                    ""severity_counts"": {{
                        ""terms"": {{
                            ""field"": ""SEVERITY.keyword"",
                            ""size"": 10
                        }}
                    }}
                }}
            }}";

        return severityQuery;
    }

    private string getTaskQuery(int workItem_id)
    {
        string taskQuery = $@"
            {{
                ""query"": {{
                    ""bool"": {{
                        ""must"": [
                            {{
                                ""term"": {{
                                    ""WORK_ITEM_ID"": {workItem_id}
                                }}
                            }},
                            {{
                                ""term"": {{
                                    ""STREAM_NAME.keyword"": ""completed_work_items""
                                }}
                            }}
                        ]
                    }}
                }}
            }}";

        return taskQuery;
    }

    private object getSeverityCountList(JsonElement severityResponse)
    {
        // Extract severity counts
        var severityBuckets = severityResponse.GetProperty("aggregations")
                                            .GetProperty("severity_counts")
                                            .GetProperty("buckets");

        var severityCountList = severityBuckets.EnumerateArray().Select(bucket => new
        {
            severity = bucket.GetProperty("key").GetString(),
            count = bucket.GetProperty("doc_count").GetInt32()
        }).ToList();

        return severityCountList;
    }

    private WorkitemInsights gettaskDetails(JsonElement taskResponse)
    {
        var hits = taskResponse.GetProperty("hits").GetProperty("hits");

        var taskDetails = hits.EnumerateArray().Select(hit => new WorkitemInsights
        {
            TaskId = hit.GetProperty("_source").GetProperty("WORK_ITEM_ID").GetInt32(),
            OriginalEstimate = hit.GetProperty("_source").GetProperty("ORIGINAL_ESTIMATE").GetInt32(),
            CreatedDate = hit.GetProperty("_source").GetProperty("CREATED_DATE").GetString(),
            EndDate = hit.GetProperty("_source").GetProperty("CLOSED_DATE").GetString(),
            InProgressDate = hit.GetProperty("_source").GetProperty("ACTIVATED_DATE").GetString()
        }).FirstOrDefault();

        return taskDetails ?? new WorkitemInsights();
    }
}
