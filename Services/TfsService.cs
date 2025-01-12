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
                PrStatus = getPrInsights.PrStatus  
            };

            return result;
        }
         
    catch (Exception ex)
        {
            Console.WriteLine( $"Internal server error: {ex.Message}");
            return new TaskInsights{}; // Return empty array in case of error
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
