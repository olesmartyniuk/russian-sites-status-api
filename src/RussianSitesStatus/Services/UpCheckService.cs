using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;

namespace RussianSitesStatus.Services;
public class UpCheckService
{
    public StatusCakeService _statusCakeService { get; set; }
    public Storage<Site> _liteStatusStorage { get; set; }

    private static readonly List<string> _monitoringRegions = new()
    {
        "novosibirsk",
        "stockholm",
        "frankfurt",
        "tokyo",
        "warsaw",
        "hong-kong",
        "mexico-city",
        "london",
        "toronto",
        "singapore",
        "sydney"
    };

    public UpCheckService(Storage<Site> liteStatusStorage, StatusCakeService statusCakeService)
    {
        _statusCakeService = statusCakeService;
        _liteStatusStorage = liteStatusStorage;
    }

    public async Task AddUptimeCheckAsync(string siteUrl)
    {
        await AddUptimeCheckAsync(siteUrl, null);
    }

    public async Task AddUptimeCheckAsync(string siteUrl, List<string> tags)
    {
        if (!_liteStatusStorage.GetAll().Any(s => s.Name == siteUrl.NormalizeSiteName()))
        {
            var newUptimeCheckItem = BuildNewUptimeCheckItem(siteUrl, tags);
            await _statusCakeService.AddUptimeCheckItemAsync(newUptimeCheckItem);
        }
    }

    private static UptimeCheckItem BuildNewUptimeCheckItem(string url, List<string> tags = null)
    {
        var newUptimeCheckItem = new UptimeCheckItem
        {
            website_url = url,
            name = url.NormalizeSiteName(),
            check_rate = Rate.Defaul,
            test_type = TestType.HTTP,
            regions = _monitoringRegions,
            follow_redirects = true,
            tags = tags
        };
        return newUptimeCheckItem;
    }
}
