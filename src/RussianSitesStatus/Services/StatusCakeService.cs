using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RussianSitesStatus.Services;
public class StatusCakeService
{
    private readonly HttpClient _httpClient;

    public StatusCakeService(IConfiguration configuration)
    {
        var apiKey = configuration["STATUS_CAKE_API_KEY"];

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<List<UptimeChecksItem>> GetAllStatuses()
    {
        var resultSet = new List<UptimeChecksItem>();
        int batchSize = 100;
        int pageNumber = 1;
        int totalCount;
        do
        {
            var response = await _httpClient.GetAsync(Endpoints.GetAllStatuses(pageNumber, batchSize));
            var payload = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UptimeChecks>(payload);
            resultSet.AddRange(result.data);

            totalCount = result.metadata.total_count;
            pageNumber++;
        }
        while (resultSet.Count < totalCount);

        return resultSet;
    }

    public async Task AddUptimeCheckItemAsync(UptimeCheckItem uptimeCheckItem)
    {
        var response = await _httpClient.PostAsync(Endpoints.AddUptimeChecksItem, uptimeCheckItem.ToFormUrlEncodedContent());

        response.EnsureSuccessStatusCode();
    }

    public async Task<UptimeCheck> GetStatus(string id)
    {
        var response = await _httpClient.GetAsync(Endpoints.GetStatus(id));
        var payload = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<UptimeCheck>(payload);
    }

    public static class Endpoints
    {
        private static string Host = "https://api.statuscake.com";
        private static string ApiVersion = "v1";

        public static string AddUptimeChecksItem => $"{Host}/{ApiVersion}/uptime";
        public static string GetAllStatuses(int pageNumber, int limit) => $"{Host}/{ApiVersion}/uptime?page={pageNumber}&limit={limit}";
        public static string GetStatus(string id) => $"{Host}/{ApiVersion}/uptime/{id}";
    }
}
