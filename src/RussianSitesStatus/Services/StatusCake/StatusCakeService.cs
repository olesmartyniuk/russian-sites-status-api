using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.StatusCake;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RussianSitesStatus.Services.StatusCake;
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
            var response = await _httpClient.GetAsync(Endpoints.GetAllUptimeChecks(pageNumber, batchSize));
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
        var response = await _httpClient.PostAsync(Endpoints.AddUptimeCheck, uptimeCheckItem.ToFormUrlEncodedContent());

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteUptimeCheckItemAsync(string id)
    {
        var response = await _httpClient.DeleteAsync(Endpoints.DeleteUptimeCheck(id));
     
        response.EnsureSuccessStatusCode();
    }

    public async Task<UptimeCheck> GetStatus(string id)
    {
        var response = await _httpClient.GetAsync(Endpoints.GetUptimeCheck(id));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<UptimeCheck>(payload);
    }

    public async Task<IEnumerable<UptimeCheckHistoryItem>> GetHistory(string id)
    {
        var from = DateTime.UtcNow.AddMinutes(-5);
        var unixTimestamp = (int)from.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        var response = await _httpClient.GetAsync(Endpoints.GetUptimeCheckHistory(id, unixTimestamp));
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync();
        var history = JsonSerializer.Deserialize<UptimeCheckHistory>(payload);        

        return history.data;
    }

    public static class Endpoints
    {
        private static string Host = "https://api.statuscake.com";
        private static string ApiVersion = "v1";

        public static string AddUptimeCheck => $"{Host}/{ApiVersion}/uptime";
        public static string GetAllUptimeChecks(int pageNumber, int limit) => $"{Host}/{ApiVersion}/uptime?page={pageNumber}&limit={limit}";
        public static string GetUptimeCheck(string id) => $"{Host}/{ApiVersion}/uptime/{id}";
        public static string DeleteUptimeCheck(string id) => $"{Host}/{ApiVersion}/uptime/{id}";
        public static string GetUptimeCheckHistory(string id, int fromUnixTime) => $"{Host}/{ApiVersion}/uptime/{id}/history?start={fromUnixTime}";
    }
}
