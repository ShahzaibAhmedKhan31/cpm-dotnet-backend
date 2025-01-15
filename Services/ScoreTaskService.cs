using System.Text.Json;
using Microsoft.Extensions.Options;
using WebApplication1.dbdata;
public class ScoretaskService
{
    private readonly ElasticSearchService _elasticSearchService;

    private readonly ApplicationDbContext _context;
    private readonly string _indexName;

    // Constructor to inject ElasticSearchService
    public ScoretaskService(ElasticSearchService elasticSearchService, ApplicationDbContext context, IOptions<IndexesName> settings)
    {
        _elasticSearchService = elasticSearchService;
        _context = context;
        _indexName = settings.Value.TASK;
    }

    public object GetTaskScore(JsonElement workItemInsights)
    {
        return new {
            automatedScore = 4.1,
            selfScore = 3.5,
            supervisorScore = (double?)null
        };
    }

    public async Task<bool> UpdateTask(JsonElement response, string task_id)
    {
        var query = UpdateTaskQuery(response);

        try
        {
            var updateTask = await _elasticSearchService.UpdateElasticSearchDocumentQueryAsync(query, _indexName, task_id);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine( $"Internal server error: {ex.Message}");
            return false;
        }
    }

    private string UpdateTaskQuery(JsonElement response)
    {

        // Construct the final query string
        var query = $@"
            {{
                ""doc"": {response},
                ""upsert"": {response}
            }}";

        return query;
    }
}