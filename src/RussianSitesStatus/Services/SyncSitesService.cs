using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace RussianSitesStatus.Services;
public class SyncSitesService
{
    private const int BATCH_SIZE = 10;

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

        await DeleteOldSitesAsync(allSitesFromSources);
        await AddNewSites(allSitesFromSources);
    }

    private async Task DeleteOldSitesAsync(IEnumerable<string> allSitesFromSources)
    {
        var allExistingSites = _liteStatusStorage.GetAll();
        var newSites = GetUptimeCheckItemToBeAddedFromLocalStorage(allSitesFromSources);

        var numberSiteToBeDeleted = (allExistingSites.Count() + newSites.Count()) - 100;
        if (numberSiteToBeDeleted > 0)
        {
            var uptimeCheckItemIdsToBeDeleted = GetUptimeCheckItemIdsToBeDeleted(allSitesFromSources, numberSiteToBeDeleted);
            var action = async (string siteId) =>
            {
                await _statusCakeService.DeleteUptimeCheckItemAsync(siteId);
            };

            await ProccesBatchAsync(uptimeCheckItemIdsToBeDeleted, action);
        }
    }

    private async Task AddNewSites(IEnumerable<string> allSitesFromSources)
    {
        var notExistingSites = GetUptimeCheckItemToBeAddedFromLocalStorage(allSitesFromSources);
        var action = async (string siteUrl) =>
        {
            var newUptimeCheckItem = BuildNewUptimeCheckItem(siteUrl);
            await _statusCakeService.AddUptimeCheckItemAsync(newUptimeCheckItem);
        };

        await ProccesBatchAsync(notExistingSites, action);
    }

    private async Task ProccesBatchAsync(IEnumerable<string> notExistingSites, Func<string, Task> action)
    {
        var taskList = new List<Task>();
        foreach (var batch in notExistingSites.Chunk(BATCH_SIZE)) //TODOVK delete Take(1)
        {
            foreach (var item in batch)
            {
                taskList.Add(action(item));
            }

            await Task.Delay(1000);// Sleep to avoid 429
        }

        await Task.WhenAll(taskList);
    }

    private static UptimeCheckItem BuildNewUptimeCheckItem(string url)
    {
        var newUptimeCheckItem = new UptimeCheckItem
        {
            website_url = url,
            name = url.NormalizeSiteName(),
            check_rate = Rate.Defaul,
            test_type = TestType.HTTP,
            regions = new List<string> { "singapore", "novosibirsk" } //TODOVK: Provide list of regions, exists 100500 diff regions
        };
        return newUptimeCheckItem;
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

    private IEnumerable<string> GetUptimeCheckItemIdsToBeDeleted(IEnumerable<string> allSitesFromSources, int size)
    {
        var sites = _liteStatusStorage.GetAll();
        var oldSites = sites.Select(s => s.WebsiteUrl.NormilizeStringUrl()).Except(allSitesFromSources.Select(s => s.NormilizeStringUrl())).Take(size);
        return sites.Where(t => oldSites.Contains(t.WebsiteUrl.NormilizeStringUrl())).Select(t => t.Id);
    }
}
