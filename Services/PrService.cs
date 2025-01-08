using System.Text.Json;
using Microsoft.Extensions.Options;
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
                        return new GetPrDetailsApi {}; // Return empty array in case of error
                        // return Ok(new { message = "No work item data found." });
                    }
                }
                else
                {
                    Console.WriteLine("No PR found against this WorkItem.");
                    return new GetPrDetailsApi {}; // Return empty array in case of error
                    // return Ok(new { message = "No PR found against this WorkItem." });
                }
            }
        catch (Exception ex)
        {
            Console.WriteLine( $"Internal server error: {ex.Message}");
            return new GetPrDetailsApi {}; // Return empty array in case of error
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
                WorkItemId= hit.GetProperty("_source").GetProperty("WORK_ITEM_ID").GetInt32(),
                PrId= hit.GetProperty("_source").GetProperty("PR_ID").GetInt32(),
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
            return new PrDetails{}; // Return null in case of an error
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