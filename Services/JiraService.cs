using System.Text.Json;
using Microsoft.Extensions.Options;
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

    public async Task<JsonElement> getJiraInsights(string Issue_key, string displayName)
    {
        try{
            var getjiraissueinfoquery = getJiraIssueInfoQuery(Issue_key);

            var getjiraissueinfo = await _elasticSearchService.ExecuteElasticsearchQueryAsync(getjiraissueinfoquery, _indexName);

            var total_1 = getjiraissueinfo.GetProperty("hits") .GetProperty("total") .GetProperty("value").GetInt32();

             if(total_1 > 0){
                var filteredResponse = FilterResponse(getjiraissueinfo,displayName);
                return filteredResponse;
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
}

