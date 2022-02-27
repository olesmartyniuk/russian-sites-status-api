using System.Net.Http.Headers;
using System.Text.Json;
using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services;
public class StatusCakeService
{
    private readonly HttpClient _httpClient;

    public StatusCakeService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "DqBjkewguSJfdHfDCMeE"); // TODO: move API key to the settings
    }
    public async Task<UptimeChecks> GetAllStatuses()
    {
        var response = await _httpClient.GetAsync("https://api.statuscake.com/v1/uptime");
        var payload = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<UptimeChecks>(payload);                
    }

    public async Task<UptimeCheck> GetStatus(string id)
    {
        var response = await _httpClient.GetAsync($"https://api.statuscake.com/v1/uptime/{id}");
        var payload = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<UptimeCheck>(payload);                
    }
}