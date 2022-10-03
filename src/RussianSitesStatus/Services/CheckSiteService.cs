using System.Collections.Concurrent;
using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Services.Contracts;
using System.Diagnostics;
using System.Net;
using RussianSitesStatus.Extensions;

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

    private HttpClient CreateHttpClient(Region region)
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
            Proxy = proxy,
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                }
        };
        var client = new HttpClient(handler);
        client.Timeout = _timeout;
        return client;
    }

    public async Task<Check> Check(Site site, Region region, DateTime checkedAt)
    {
        var timer = new Stopwatch();
        var statusCode = -1;
        Check newCheck;

        try
        {
            var httpClient = HttpClientsByRegion.GetOrAdd(region.ProxyUrl, CreateHttpClient(region));
            timer.Start();
            var response = await httpClient.GetAsync(site.Url);
            statusCode = (int)response.StatusCode;
        }
        catch (TaskCanceledException ex)
        {
            statusCode = 0;
            // _logger.LogInformation($"Timeout error on site. Proxy url: {region.ProxyUrl}, Site: {site.Url}, Exception message: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            // _logger.LogInformation($"Proxy is not available. Proxy url: {region.ProxyUrl}, Site: {site.Url}, Exception message: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unhandled exception. Proxy url: {region.ProxyUrl}, Site: {site.Url}, Exception message: {ex.Message}, Trace: {ex.StackTrace}");
        }
        finally
        {
            timer.Stop();
            newCheck = BuildCheck(statusCode, site, region, (int)timer.Elapsed.TotalSeconds, checkedAt);
        }
        return newCheck;
    }

    public async Task<Site> CheckByUrl(string siteUrl, IEnumerable<Region> regions)
    {
        var checkedAt = DateTime.UtcNow;
        var regionsById = regions
            .ToDictionary(r => r.Id, r => r);

        var site = new Site
        {
            Id = 0,
            Name = siteUrl.NormalizeSiteName(),
            Url = siteUrl.NormalizeSiteUrl(),
            CheckedAt = checkedAt
        };

        var checkTasks = new List<Task<Check>>();

        foreach (var region in regions)
        {
            var checkTask = Check(site, region, checkedAt);
            checkTasks.Add(checkTask);
        }

        var checks = await Task.WhenAll(checkTasks);

        foreach (var check in checks)
        {
            check.Region = regionsById[check.RegionId];
            site.Checks.Add(check);
        }        

        return site;
    }

    public Check BuildCheck(int statusCode, Site site, Region region, int spentTime, DateTime checkedAt)
    {
        return new Check
        {
            CheckedAt = checkedAt,
            SiteId = site.Id,
            StatusCode = statusCode,
            SpentTime = spentTime,
            RegionId = region.Id,
            Status = GetStatus(statusCode)
        };        
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
