using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;


public class WebHrService
{
    private readonly HttpClient _httpClient;
    private readonly string _url;

    // Inject HttpClient via constructor
    public WebHrService(HttpClient httpClient, IOptions<WebHrSettings> settings)
    {
        _httpClient = httpClient;
        _url = settings.Value.Url;
    }

    public 

}

