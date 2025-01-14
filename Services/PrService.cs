using System.Text.Json;
using Microsoft.Extensions.Options;
using PullRequest.Models;
public class PrService
{
    private readonly ElasticSearchService _elasticSearchService;
    private readonly string _indexName;

    // Constructor to inject ElasticSearchService
    public PrService(ElasticSearchService elasticSearchService, IOptions<IndexesName> settings)
    {
        _elasticSearchService = elasticSearchService;
        _indexName = settings.Value.PR;
    }

    public async Task<GetPrDetailsApi> GetPrInsights(int work_item_id)
    {
        Console.WriteLine("in GetPrInsights Service Functions");
        try
        {

            // Get PR ID query based on work item ID
            var get_pr_id_query = getPrIdQuery(work_item_id);

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
                var workItemPrData = getWorkItemPrData(response_1);

                // Ensure that workItemPrData contains data before proceeding
                if (workItemPrData.Any())
                {
                    // Get the PR details query based on the first item
                    var get_pr_details_query = GetPrDetailsQuery(workItemPrData[0].PrId);

                    // Execute Elasticsearch query for PR details
                    var response_2 = await _elasticSearchService.ExecuteElasticsearchQueryAsync(get_pr_details_query, _indexName);

                    var total_2 = response_2
                        .GetProperty("hits")
                        .GetProperty("total")
                        .GetProperty("value").GetInt32();

                    // Return the response if PR details are found
                    if (total_2 > 0)
                    {
                        var pr_details = getPrDetails(response_2);

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

                        return createPrDetailsResponse;
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
                        return createPrDetailsResponse;
                    }
                }
                else
                {
                    Console.WriteLine("No work item data found.");
                    return new GetPrDetailsApi { }; // Return empty array in case of error
                                                    // return Ok(new { message = "No work item data found." });
                }
            }
            else
            {
                Console.WriteLine("No PR found against this WorkItem.");
                return new GetPrDetailsApi { }; // Return empty array in case of error
                                                // return Ok(new { message = "No PR found against this WorkItem." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return new GetPrDetailsApi { }; // Return empty array in case of error
        }
    }

    public async Task<List<GetPrCountByMonthApiResponse>> GetPrCountByMonth(string date, string username)
    {
        var query = getPrCountByMonthQuery(date, username);

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

            return monthlyData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return new List<GetPrCountByMonthApiResponse>(); // Return empty array in case of error
        }
    }

    public async Task<List<GetPrWithCommentsCountApiResponse>> getPrWithCommentsCount(string date, string username)
    {
        var query = getPrWithCommentsCountQuery(date, username);

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
            return prDetails;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return new List<GetPrWithCommentsCountApiResponse>(); // Return empty array in case of error
        }
    }

    public async Task<List<GetReviewedPrCountApiResponse>> getReviewedPrCount(string date, string username)
    {
        var query = GetReviewedPrCountQuery(date, username);

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
            return monthlyData;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Internal server error: {ex.Message}");
            return new List<GetReviewedPrCountApiResponse>(); // Return empty array in case of error
        }
    }
    public string getPrIdQuery(int workItemId)
    {
        Console.WriteLine("in GetPrDetails Service Functions");
        var query = $@"
                        {{
                            ""_source"": [
                                ""PR_ID"",
                                ""WORK_ITEM_ID"",
                                ""CREATEDBYNAME"",
                                ""CREATEDBYEMAIL"",
                                ""TITLE"",
                                ""CREATEDDATE""
                            ],
                            ""query"": {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""TABLE_NAME.keyword"": ""PR_WORKITEM_TABLE""
                                            }}
                                        }},
                                        {{
                                            ""term"": {{
                                                ""WORK_ITEM_ID"": ""{workItemId}""
                                            }}
                                        }}
                                    ]
                                }}
                            }}
                        }}";

        return query;
    }

    public string GetPrDetailsQuery(int PR_ID)
    {
        Console.WriteLine("in GetPrDetails Service Functions");
        var query = $@"
                        {{
                            ""_source"": [
                                ""PR_ID"",
                                ""LAST_MERGE_COMMIT_ID"",
                                ""TOTAL_NUMBER_OF_COMMENTS"",
                                ""PR_CLOSE_DATE"",
								""PR_FIRST_COMMENT_DATE"",
								""PR_STATUS"",
								""CLOSED_BY_NAME""
                            ],
                            ""query"": {{
                                ""bool"": {{
                                    ""must"": [
                                        {{
                                            ""term"": {{
                                                ""TABLE_NAME.keyword"": ""PR_COMPLETED_STREAM""
                                            }}
                                        }},
                                        {{
                                            ""term"": {{
                                                ""PR_ID"": {PR_ID}
                                            }}
                                        }}
                                    ]
                                }}
                            }}
                        }}";

        return query;
    }

    public List<WorkItemPrData> getWorkItemPrData(JsonElement jsonResponse)
    {
        Console.WriteLine("in getWorkItemPrData Service Functions");
        try
        {
            // Extract hits from the response
            var hits = jsonResponse.GetProperty("hits").GetProperty("hits");

            // Transform the hits into an array of JSON objects
            var result = hits.EnumerateArray().Select(hit => new WorkItemPrData
            {
                CreatedByEmail = hit.GetProperty("_source").GetProperty("CREATEDBYEMAIL").GetString(),
                CreatedDate = hit.GetProperty("_source").GetProperty("CREATEDDATE").GetString(),
                CreatedByName = hit.GetProperty("_source").GetProperty("CREATEDBYNAME").GetString(),
                WorkItemId = hit.GetProperty("_source").GetProperty("WORK_ITEM_ID").GetInt32(),
                PrId = hit.GetProperty("_source").GetProperty("PR_ID").GetInt32(),
                Title = hit.GetProperty("_source").GetProperty("TITLE").GetString()
            }).ToList();

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing response: {ex.Message}");
            return new List<WorkItemPrData>(); // Return empty array in case of error
        }
    }


    public PrDetails getPrDetails(JsonElement jsonResponse)
    {
        Console.WriteLine("in getPrDetails Service Functions");
        try
        {
            // Extract hits from the response
            var hits = jsonResponse.GetProperty("hits").GetProperty("hits");

            // Get the first hit (or apply some condition to select a specific hit)
            var firstHit = hits.EnumerateArray().FirstOrDefault();

            // Transform the first hit into a PrDetails object
            var result = new PrDetails
            {
                TotalNumberOfComments = firstHit.GetProperty("_source").GetProperty("TOTAL_NUMBER_OF_COMMENTS").GetInt32(),
                PrClosedDate = firstHit.GetProperty("_source").GetProperty("PR_CLOSE_DATE").GetString(),
                LastMergeCommitId = firstHit.GetProperty("_source").GetProperty("LAST_MERGE_COMMIT_ID").GetString(),
                PrClosedByName = firstHit.GetProperty("_source").GetProperty("CLOSED_BY_NAME").GetString(),
                PrFirstCommentDate = firstHit.GetProperty("_source").GetProperty("PR_FIRST_COMMENT_DATE").GetString(),
                PrStatus = firstHit.GetProperty("_source").GetProperty("PR_STATUS").GetString()
            };

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing response: {ex.Message}");
            return new PrDetails { }; // Return null in case of an error
        }
    }

    public List<Dictionary<string, string>> FilterPrDetails(JsonElement jsonResponse)
    {
        Console.WriteLine("in FilterPrDetails Service Functions");
        var fieldsToKeep = new[]
        {
            "PR_ID",
            "WORK_ITEM_ID",
            "CREATEDBYNAME",
            "CREATEDBYEMAIL",
            "TITLE",
            "CLOSED_BY_NAME",
            "PR_CLOSE_DATE",
            "PR_CREATION_DATE",
            "TOTAL_NUMBER_OF_COMMENTS",
            "LAST_MERGE_COMMIT_ID"
        };

        var filteredData = jsonResponse
            .GetProperty("hits")
            .GetProperty("hits")
            .EnumerateArray()
            .Select(hit =>
            {
                var source = hit.GetProperty("_source");

                return fieldsToKeep.ToDictionary(
                    field => field,
                    field => source.TryGetProperty(field, out var value) ? value.ToString() : string.Empty
                );
            })
            .ToList();

        return filteredData;
    }


    public async Task<IEnumerable<object>> GetPrCountByName(List<string> names, int months)
    {
        // Generate the query using the updated method that handles names
        var query = getPRcountByNameQuery(names, months);
        Console.WriteLine(query);

        try
        {
            // Execute the Elasticsearch query
            var searchResponse = await _elasticSearchService.ExecuteElasticsearchQueryAsync(query, _indexName);
            Console.WriteLine(searchResponse);

            // Access and parse the response aggregations
            var aggregations = searchResponse.GetProperty("aggregations");
            var monthBuckets = aggregations.GetProperty("pr_count_by_month").GetProperty("buckets");

            // Transform the month buckets into the desired result format
            var result = monthBuckets.EnumerateArray().Select(monthBucket => new
            {
                month = monthBucket.GetProperty("key_as_string").GetString(),
                totalPRCount = monthBucket.GetProperty("doc_count").GetInt32(),
                creators = monthBucket.GetProperty("by_creator").GetProperty("buckets").EnumerateArray().Select(creatorBucket => new
                {
                    creatorName = creatorBucket.GetProperty("key").GetString(),
                    prCount = creatorBucket.GetProperty("doc_count").GetInt32()
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

    private static string getPRcountByNameQuery(List<string> names, int months)
    {
        // Construct the "wildcard" conditions for multiple names dynamically
        var creatorNameConditions = string.Join(",", names.Select(name => $@"
        {{
            ""wildcard"": {{
                ""CREATED_BY_NAME.keyword"": ""*{name}*""
            }}
        }}"));

        // Construct the query as a JSON string
        string query = $@"
    {{
        ""size"": 0,
        ""query"": {{
            ""bool"": {{
                ""must"": [
                    {{
                        ""term"": {{
                            ""PR_STATUS.keyword"": ""completed""
                        }}
                    }},
                    {{
                        ""bool"": {{
                            ""should"": [
                                {creatorNameConditions}
                            ],
                            ""minimum_should_match"": 1
                        }}
                    }}
                ],
                ""filter"": [
                    {{
                        ""range"": {{
                            ""PR_CLOSE_DATE"": {{
                                ""gte"": ""now-{months}M/M"",
                                ""lte"": ""now/M""
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
                }},
                ""aggs"": {{
                    ""by_creator"": {{
                        ""terms"": {{
                            ""field"": ""CREATED_BY_NAME.keyword"",
                            ""size"": 10
                        }}
                    }}
                }}
            }}
        }}
    }}";

        return query;
    }

    private static string getPrCountByMonthQuery(string date, string username)
    {
        var query = $@"
            {{
                ""query"": {{
                    ""bool"": {{
                        ""must"": [
                            {{
                                ""term"": {{
                                    ""TABLE_NAME.keyword"": ""PR_COMPLETED_STREAM""
                                }}
                            }},
                            {{
                                ""term"": {{
                                    ""PR_STATUS.keyword"": ""completed""
                                }}
                            }},
                            {{
                                ""term"": {{
                                    ""CREATED_BY_NAME.keyword"": ""{username}""
                                }}
                            }}
                        ],
                        ""filter"": [
                            {{
                                ""range"": {{
                                    ""PR_CLOSE_DATE"": {{
                                        ""gte"": ""{date}""
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

        return query;
    }

    private static string getPrWithCommentsCountQuery(string date, string username)
    {
        var query = $@"
            {{
            ""_source"": [""PR_ID"", ""TOTAL_NUMBER_OF_COMMENTS"", ""PR_TITLE""],
            ""query"": {{
                ""bool"": {{
                ""must"": [
                    {{
                        ""term"": {{
                            ""TABLE_NAME.keyword"": ""PR_COMPLETED_STREAM""
                        }}
                    }},
                    {{
                        ""term"": {{
                            ""CREATED_BY_NAME.keyword"": ""{username}""
                        }}
                    }}
                ],
                ""filter"": [
                    {{
                    ""range"": {{
                        ""PR_CREATION_DATE"": {{
                        ""gte"": ""{date}"",
                        ""lte"": ""now/M""
                        }}
                    }}
                    }}
                ]
                }}
            }},
            ""size"": 100
            }}";

        return query;
    }

    private static string GetReviewedPrCountQuery(string date, string username)
    {
        var query = $@"
            {{
            ""query"": {{
                ""bool"": {{
                ""must"": [
                    {{
                        ""term"": {{
                            ""TABLE_NAME.keyword"": ""PR_COMPLETED_STREAM""
                        }}
                    }},
                    {{
                        ""term"": {{
                            ""CLOSED_BY_NAME.keyword"": ""{username}""
                        }}
                    }}
                ],
                ""filter"": [
                    {{
                    ""range"": {{
                        ""PR_CLOSE_DATE"": {{
                        ""gte"": ""{date}"",
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

        return query;
    }

}

// Models To be used by prServices

public class WorkItemPrData
{
    public string? CreatedByEmail { get; set; }
    public string? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int WorkItemId { get; set; }
    public int PrId { get; set; }
    public string? Title { get; set; }
}

public class PrDetails
{
    public int TotalNumberOfComments { get; set; }
    public string? LastMergeCommitId { get; set; }
    public string? PrClosedDate { get; set; }
    public string? PrClosedByName { get; set; }
    public string? PrFirstCommentDate { get; set; }
    public string? PrStatus { get; set; }

}

public class GetPrDetailsApi
{
    public string? CreatedByEmail { get; set; }
    public string? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int WorkItemId { get; set; }
    public int PrId { get; set; }
    public string? Title { get; set; }
    public string? TotalNumberOfComments { get; set; }
    public string? LastMergeCommitId { get; set; }
    public string? PrClosedDate { get; set; }
    public string? PrClosedByName { get; set; }
    public string? PrFirstCommentDate { get; set; }
    public string? PrStatus { get; set; }

}