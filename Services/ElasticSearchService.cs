// Services/ElasticSearchService.cs
using Nest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JiraApi.Services
{
    public class ElasticSearchService
    {
        private readonly ElasticClient _client;

        public ElasticSearchService(IConfiguration configuration)
        {
            var settings = new ConnectionSettings(new Uri(configuration["ElasticSearch:Url"]))
                           .DefaultIndex("jira_completed_issues_new");  // Default index

            _client = new ElasticClient(settings);
        }

        public ElasticClient Client => _client;
    }
}
