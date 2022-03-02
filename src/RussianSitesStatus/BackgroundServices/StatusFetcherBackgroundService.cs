using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.BackgroundServices;

public class StatusFetcherBackgroundService : BackgroundService
{
    private const int WAIT_TO_NEXT_CHECK_SECONDS = 30;
    private const int REQUESTS_PER_SECOND_LIMIT = 10;
    private const int ONE_SECOND = 1000;

    private readonly StatusCakeService _statusCakeService;
    private readonly Storage<Site> _liteStatusStorage;
    private readonly Storage<SiteDetails> _fullStatusStorage;
    private readonly ILogger<StatusFetcherBackgroundService> _logger;

    public StatusFetcherBackgroundService(
        StatusCakeService statusCakeService,
        Storage<Site> liteStatusStorage,
        Storage<SiteDetails> fullStatusStorage,
        ILogger<StatusFetcherBackgroundService> logger)
    {
        _statusCakeService = statusCakeService;
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var statuses = await UpdateLiteStatuses();
                await UpdateFullStatuses(statuses);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            await Task.Delay(TimeSpan.FromSeconds(WAIT_TO_NEXT_CHECK_SECONDS), stoppingToken);
        }
    }

    private async Task UpdateFullStatuses(IEnumerable<Site> statuses)
    {
        foreach (var batch in statuses.Chunk(REQUESTS_PER_SECOND_LIMIT))
        {
            foreach (var status in batch)
            {
                var uptimeCheck = await _statusCakeService.GetStatus(status.Id);
                var history = await GetHistory(status);

                var data = uptimeCheck.data;
                var siteStatus = new SiteDetails
                {
                    Id = data.id,
                    Name = data.name.NormalizeSiteName(),
                    Status = data.status,
                    TestType = data.test_type,
                    Uptime = data.uptime,
                    WebsiteUrl = data.website_url,
                    DoNotFind = data.do_not_find,
                    LastTestedAt = data.last_tested_at,
                    Processing = data.processing,
                    Servers = data.servers.Select(s => new Server
                    {
                        Region = s.region,
                        Status = GetStatusByRegion(history, s.region_code),
                        StatusCode = GetStatusCodeByRegion(history, s.region_code),
                        LastTestedAt = GetLastTestedAtByRegion(history, s.region_code)
                    }).ToList(),
                    Timeout = data.timeout
                };

                _fullStatusStorage.Replace(siteStatus);                
            }

            await Task.Delay(ONE_SECOND); // Sleep to avoid 429
        }

        string GetStatusByRegion(Dictionary<string, UptimeCheckHistoryItem> history, string regionCode)
        {
            if (!history.ContainsKey(regionCode))
            {
                return string.Empty;
            }

            var statusCode = history[regionCode].status_code;

            if (statusCode >= 200 && statusCode <= 300)
            {
                return "up";
            }

            return "down";
        }

        int GetStatusCodeByRegion(Dictionary<string, UptimeCheckHistoryItem> history, string regionCode)
        {
            if (!history.ContainsKey(regionCode))
            {
                return -1;
            }

            return history[regionCode].status_code;
        }

        DateTime GetLastTestedAtByRegion(Dictionary<string, UptimeCheckHistoryItem> history, string regionCode)
        {
            if (!history.ContainsKey(regionCode))
            {
                return default(DateTime);
            }

            return history[regionCode].created_at;
        }
    }

    private async Task<Dictionary<string, UptimeCheckHistoryItem>> GetHistory(Site status)
    {
        var result = new Dictionary<string, UptimeCheckHistoryItem>();
        var history = await _statusCakeService.GetHistory(status.Id);

        foreach (var historyItem in history)
        {
            if (!result.ContainsKey(historyItem.location))
            {
                result.Add(historyItem.location, historyItem);
            }
        }

        return result.Values
            .Where(value => !string.IsNullOrEmpty(GetRegionByLocation(value.location)))
            .ToDictionary(value => GetRegionByLocation(value.location));
    }

    private string GetRegionByLocation(string location)
    {
    return location.ToUpper() switch
        {
            "RU3" => "novosibirsk",
           
            "SG1" => "singapore",
            "SG2" => "singapore",
            
            "SWE1" => "stockholm",
            "SE3" => "stockholm",
                       
            "DEFR-1" => "frankfurt",
            "DODE6" => "frankfurt",

            "BR1" => "sao-paulo",
            
            "JP1" => "tokyo",
            "JP5" => "tokyo",
            
            "PL4" => "warsaw",
            "PL2" => "warsaw",
            
            "HK" => "hong-kong",
            "HK2" => "hong-kong",

            "MEX" => "mexico-city",
            "MEX2" => "mexico-city",
            
            "UKBOB" => "london",
            "FREE12SUB1" => "london",
            
            "TORO3" => "toronto",
            "CATOR" => "toronto",
            
            "AU4" => "sydney",
            "AU5" => "sydney",

            _ => string.Empty
        };
    }

    private async Task<IEnumerable<Site>> UpdateLiteStatuses()
    {
        var statuses = await _statusCakeService.GetAllStatuses();

        var siteStatuses = statuses.Select(status => new Site
        {
            Id = status.id,
            Name = status.name.NormalizeSiteName(),
            Status = status.status,
            TestType = status.test_type,
            Uptime = status.uptime,
            WebsiteUrl = status.website_url
        });

        _liteStatusStorage.ReplaceAll(siteStatuses);

        return siteStatuses;
    }
}