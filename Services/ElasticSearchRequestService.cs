using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;


public class ElasticSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _elasticsearchUrl;

    // Inject HttpClient via constructor
    public ElasticSearchService(HttpClient httpClient, IOptions<ElasticsearchSettings> settings)
    {
        _httpClient = httpClient;
        _elasticsearchUrl = settings.Value.Url;
        Console.WriteLine($"Elasticsearch URL: {_elasticsearchUrl}"); // Log or debug the URL

    }

    // Your logic for interacting with Elasticsearch
    public async Task<JsonElement> ExecuteElasticsearchQueryAsync(string query, string index)
    {
        var uri = $"{_elasticsearchUrl}/{index}/_search";
        var httpContent = new StringContent(query, Encoding.UTF8, "application/json");

        var httpResponse = await _httpClient.PostAsync(uri, httpContent);

        if (!httpResponse.IsSuccessStatusCode)
        {
            var error = await httpResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error from Elasticsearch: {error}");
        }

        var responseContent = await httpResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(responseContent);
    }
}

