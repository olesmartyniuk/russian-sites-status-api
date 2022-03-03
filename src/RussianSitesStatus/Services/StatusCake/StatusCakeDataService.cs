using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.Services.StatusCake;
public class StatusCakeDataService : IDataService
{
    private const int REQUESTS_PER_SECOND_LIMIT = 10;
    private const int ONE_SECOND = 1000;

    private readonly StatusCakeService _statusCakeService;

    public StatusCakeDataService(StatusCakeService statusCakeService)
    {
        _statusCakeService = statusCakeService;
    }

    public async Task<IEnumerable<SiteVM>> GetAllAsync()
    {
        var statuses = await _statusCakeService.GetAllStatuses();

        var siteStatuses = statuses.Select(status => new SiteVM
        {
            Id = status.id,
            Name = status.name.NormalizeSiteName(),
            Status = status.status,
            TestType = status.test_type,
            Uptime = status.uptime,
            WebsiteUrl = status.website_url,
            Tags = status.tags
        });

        return siteStatuses;
    }

    public async Task<IEnumerable<SiteDetailsVM>> GetAllSitesDetailsAsync(IEnumerable<SiteVM> liteModels = null)
    {
        var result = new List<SiteDetailsVM>();
        if (liteModels == null)
        {
            var allSites = await GetAllAsync();
        }

        foreach (var batch in liteModels.Chunk(REQUESTS_PER_SECOND_LIMIT))
        {
            foreach (var status in batch)
            {
                var uptimeCheck = await _statusCakeService.GetStatus(status.Id);
                var history = await GetHistory(status);

                var data = uptimeCheck.data;
                var siteStatus = new SiteDetailsVM
                {
                    Id = data.id,
                    Name = data.name.NormalizeSiteName(),
                    Status = data.status,
                    TestType = data.test_type,
                    Uptime = data.uptime,
                    WebsiteUrl = data.website_url,
                    LastTestedAt = data.last_tested_at,
                    Servers = data.servers.Select(s => new ServerDto
                    {
                        Region = s.region,
                        RegionCode = s.region_code,
                        Status = GetStatusByRegion(history, s.region_code),
                        StatusCode = GetStatusCodeByRegion(history, s.region_code),
                        LastTestedAt = GetLastTestedAtByRegion(history, s.region_code)
                    }).ToList(),
                    Timeout = data.timeout
                };

                result.Add(siteStatus);
            }

            await Task.Delay(ONE_SECOND); // Sleep to avoid 429
        }

        return result;
    }

    private static string GetStatusByRegion(Dictionary<string, UptimeCheckHistoryItem> history, string regionCode)
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

    private static int GetStatusCodeByRegion(Dictionary<string, UptimeCheckHistoryItem> history, string regionCode)
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

    private async Task<Dictionary<string, UptimeCheckHistoryItem>> GetHistory(SiteVM status)
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
}
