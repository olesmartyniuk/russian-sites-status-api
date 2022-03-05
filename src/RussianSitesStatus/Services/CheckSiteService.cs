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
    private readonly ILogger<CheckSiteService> _logger;

    private static readonly ConcurrentDictionary<string, HttpClient> HttpClientsByRegion = new();

    public CheckSiteService(ILogger<CheckSiteService> logger)
    {
        _logger = logger;
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

    public async Task<Check> CheckAsync(SiteVM site, RegionVM region, Guid iteration)
    {
        var timer = new Stopwatch();
        var statusCode = -1;
        Check newCheck;

        try
        {
            var httpClient = HttpClientsByRegion.GetOrAdd(region.ProxyUrl, CreateHttpClient(region));
            timer.Start();
            var response = await httpClient.GetAsync(site.WebsiteUrl);
            statusCode = (int)response.StatusCode;
        }
        catch (TaskCanceledException ex)
        {
            statusCode = 0;
            _logger.LogError($"Timeout error on site. Proxy url: {region.ProxyUrl}, Site: {site.WebsiteUrl}, Exception message: {ex.Message}, Trace: {ex.StackTrace}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Proxy is not available. Proxy url: {region.ProxyUrl}, Site: {site.WebsiteUrl}, Exception message: {ex.Message}, Trace: {ex.StackTrace}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unhandled exception. Proxy url: {region.ProxyUrl}, Site: {site.WebsiteUrl}, Exception message: {ex.Message}, Trace: {ex.StackTrace}");
        }
        finally
        {
            timer.Stop();
            newCheck = BuildCheck(statusCode, site, region, (int)timer.Elapsed.TotalSeconds, iteration);
        }
        return newCheck;

    }

    public Check BuildCheck(int statusCode, SiteVM site, RegionVM region, int spentTime, Guid iteration)
    {
        var check = new Check
        {
            CheckedAt = DateTime.UtcNow,
            SiteId = int.Parse(site.Id),
            StatusCode = statusCode,
            SpentTime = spentTime,
            RegionId = region.Id,
            Iteration = iteration,
            Status = GetStatus(statusCode)
        };
        return check;
    }
    
    private CheckStatus GetStatus(int statusCode)
    {
        var status = statusCode switch
        {
            -1 => CheckStatus.Unknown,
            >= 200 and <= 300 => CheckStatus.Available,
            _ => CheckStatus.Unavailable
        };
        return status;
    }
}
