using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;

namespace RussianSitesStatus.Services;
public class UpCheckService
{
    public StatusCakeService _statusCakeService { get; set; }
    public Storage<SiteDto> _liteStatusStorage { get; set; }

    public UpCheckService(Storage<SiteDto> liteStatusStorage, StatusCakeService statusCakeService)
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
            tags = tags,
            test_type = TestType.HTTP,
            regions = new List<string> { "singapore", "novosibirsk" } //TODOVK: Provide list of regions, exists 100500 diff regions
        };
        return newUptimeCheckItem;
    }
}
