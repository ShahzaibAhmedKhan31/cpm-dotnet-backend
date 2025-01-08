using System.Text.Json;

public class PrService
{
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
        Console.WriteLine("Response 1: " + jsonResponse.ToString()); // Ensure correct logging of response
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
    Console.WriteLine("In getPrDetails"); // Ensure correct logging of response
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
        return null; // Return null in case of an error
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
