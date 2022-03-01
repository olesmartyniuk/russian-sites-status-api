using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace RussianSitesStatus.Services;
public class SyncSitesService
{
    private readonly HttpClient _httpClient;
    private readonly IEnumerable<ISiteSource> _siteSources;
    private readonly StatusCakeService _statusCakeService;
    private readonly Storage<Site> _liteStatusStorage;
    private readonly ILogger<SyncSitesService> _logger;

    public SyncSitesService(IConfiguration configuration,
        IEnumerable<ISiteSource> siteSources,
        StatusCakeService statusCakeService,
        Storage<Site> liteStatusStorage,
        ILogger<SyncSitesService> logger)
    {
        var apiKey = configuration["STATUS_CAKE_API_KEY"];

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _siteSources = siteSources;
        _statusCakeService = statusCakeService;
        _liteStatusStorage = liteStatusStorage;
        _logger = logger;
    }

    public async Task SyncAsync()
    {
        var allSitesFromSources = await GetSitesFromAllSources();
        var notExistingSites = GetUptimeCheckItemToBeAddedFromLocalStorage(allSitesFromSources);

        var taskList = new List<Task>();
        foreach (var item in notExistingSites.Take(1))//TODOVK: Sleep to avoid 429
        {
            var action = async () =>
            {
                var newUptimeCheckItem = new UptimeCheckItem
                {
                    website_url = item,
                    name = item.NormalizeSiteName(),
                    check_rate = Rate.Defaul,
                    test_type = TestType.HTTP,
                    regions = new List<string> { "singapore", "novosibirsk" } //TODOVK: Provide list of regions, exists 100500 diff regions
                };

                await _statusCakeService.AddUptimeCheckItemAsync(newUptimeCheckItem);
            };

            taskList.Add(Task.Run(action));
        }

        await Task.WhenAll(taskList);
    }

    private async Task<IEnumerable<string>> GetSitesFromAllSources()
    {
        var allSites = new ConcurrentBag<string>();
        var taskList = new List<Task>();
        foreach (var siteSource in _siteSources)
        {
            var action = async () =>
            {
                try
                {
                    var sites = (await siteSource.GetAllAsync()).Where(url => url.IsValid());
                    allSites.Add(sites);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $@"Unhandled exception while siteUrls from {siteSource.GetType().Name}");
                }
            };

            taskList.Add(Task.Run(action));
        }

        await Task.WhenAll(taskList);
        return allSites.ToList();
    }

    private IEnumerable<string> GetUptimeCheckItemToBeAddedFromLocalStorage(IEnumerable<string> allSitesFromSources)
    {
        var sites = _liteStatusStorage.GetAll();
        var notExistingSites = allSitesFromSources.Select(s => s.NormilizeStringUrl()).Except(sites.Select(s => s.WebsiteUrl.NormilizeStringUrl()));
        return notExistingSites;
    }
}
