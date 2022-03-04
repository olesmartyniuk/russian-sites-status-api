using System.Collections.Concurrent;
using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;
using System.Diagnostics;
using System.Net;

namespace RussianSitesStatus.Services;
public class CheckSiteService : ICheckSiteService
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30); 
    private readonly DatabaseStorage _databaseStorage;
    private readonly ILogger<CheckSiteService> _logger;
    private static readonly ConcurrentDictionary<string, HttpClient> HttpClientsByRegion = new();

    public CheckSiteService(IServiceScopeFactory serviceScopeFactory, ILogger<CheckSiteService> logger)
    {
        _logger = logger;
        using var serviceScope = serviceScopeFactory.CreateScope();
        _databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();
    }

    private HttpClient CreateHttpClient(RegionVM region)
    {
        var proxy = new WebProxy
        {
            Address = new Uri(region.ProxyUrl),
            BypassProxyOnLocal = false,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(region.ProxyUser, region.ProxyPassword)
        };
        var handler = new HttpClientHandler
        {
            Proxy = proxy
        };
        var client = new HttpClient(handler);
        client.Timeout = _timeout;
        return client;
    }

    public async Task CheckAsync(SiteVM site, RegionVM region)
    {
        var timer = new Stopwatch();
        HttpResponseMessage response = null;
        try
        {
            var httpClient = HttpClientsByRegion.GetOrAdd(region.ProxyUrl, CreateHttpClient(region));
            timer.Start();
            response = await httpClient.GetAsync(site.WebsiteUrl);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Proxy is not available. Proxy url: {region.ProxyUrl}, Exception message: {ex.Message}, Trace: {ex.StackTrace}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unhandled exception. Proxy url: {region.ProxyUrl}, Exception message: {ex.Message}, Trace: {ex.StackTrace}");
        }
        finally
        {
            timer.Stop();
            var statusCode = response is null ? -1 : (int)response.StatusCode;
            await SaveCheckAsync(statusCode, site, region, timer.Elapsed.Seconds);
        }   
    }
    
    public async Task SaveCheckAsync(int statusCode, SiteVM site, RegionVM region, int spentTime)
    {
        var check = new Check
        {
            CheckedAt = DateTime.UtcNow,
            SiteId = int.Parse(site.Id),
            StatusCode = statusCode,
            SpentTime = spentTime,
            RegionId = region.Id
        };

        await _databaseStorage.AddCheck(check);
    }
}
