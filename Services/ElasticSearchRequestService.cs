using System.Text;
using System.Text.Json;


public class ElasticSearchService
{
    private readonly HttpClient _httpClient;

    // Inject HttpClient via constructor
    public ElasticSearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Your logic for interacting with Elasticsearch
    public async Task<JsonElement> ExecuteElasticsearchQueryAsync(string query, string index)
    {
        var uri = $"http://localhost:9200/{index}/_search";
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

