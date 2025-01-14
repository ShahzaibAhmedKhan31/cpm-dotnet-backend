using System.Text.Json;
using Microsoft.Extensions.Options;
using WebApplication1.dbdata;
public class ScoretaskService
{
    private readonly ElasticSearchService _elasticSearchService;

    private readonly ApplicationDbContext _context;

    // Constructor to inject ElasticSearchService
    public ScoretaskService(ElasticSearchService elasticSearchService, ApplicationDbContext context)
    {
        _elasticSearchService = elasticSearchService;
        _context = context;
    }

    public object GetTaskScore(JsonElement workItemInsights)
    {
        return new {
            automatedScore = 4.1,
            selfScore = 3.5,
            supervisorScore = (double?)null
        };
    }
}