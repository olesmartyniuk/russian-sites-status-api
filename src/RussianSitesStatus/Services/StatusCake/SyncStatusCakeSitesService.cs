using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants.StatusCake;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace RussianSitesStatus.Services.StatusCake;
public class SyncStatusCakeSitesService : ISyncSitesService
{
    private const int REQUESTS_PER_SECOND_LIMIT = 10;
    private const int ONE_SECOND = 1000;

    private readonly HttpClient _httpClient;
    private readonly IEnumerable<ISiteSource> _siteSources;
    private readonly StatusCakeService _statusCakeService;
    private readonly Storage<SiteVM> _liteStatusStorage;
    private readonly StatusCakeUpCheckService _upCheckService;
    private readonly ILogger<SyncStatusCakeSitesService> _logger;
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

    public SyncStatusCakeSitesService(IConfiguration configuration,
        IEnumerable<ISiteSource> siteSources,
        StatusCakeService statusCakeService,
        Storage<SiteVM> liteStatusStorage,
        ILogger<SyncStatusCakeSitesService> logger,
        StatusCakeUpCheckService upCheckService)
    {
        var apiKey = configuration["STATUS_CAKE_API_KEY"];

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        _siteSources = siteSources;
        _statusCakeService = statusCakeService;
        _liteStatusStorage = liteStatusStorage;
        _logger = logger;
        _upCheckService = upCheckService;
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

        await ProccesBatchAsync(notExistingSites, _upCheckService.AddUptimeCheckAsync);
    }

    private async Task ProccesBatchAsync(IEnumerable<string> notExistingSites, Func<string, Task> action)
    {
        var taskList = new List<Task>();
        foreach (var batch in notExistingSites.Chunk(REQUESTS_PER_SECOND_LIMIT))
        {
            foreach (var item in batch)
            {
                taskList.Add(action(item));
            }

            await Task.Delay(ONE_SECOND); // Sleep to avoid 429
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
                    var sites = (await siteSource.GetAllAsync())
                        .Where(url => url.IsValid());

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
        var notExistingSites = allSitesFromSources
            .Select(s => s.NormilizeStringUrl())
            .Except(sites.Select(s => s.WebsiteUrl.NormilizeStringUrl()));
        return notExistingSites;
    }

    private IEnumerable<string> GetUptimeCheckItemIdsToBeDeleted(IEnumerable<string> allSitesFromSources, int size)
    {
        var sites = _liteStatusStorage.GetAll();
        var oldSites = sites
            .Where(s => !s.Tags.Any(t => t == Tag.CustomSite))
            .Select(s => s.WebsiteUrl.NormilizeStringUrl())
            .Except(allSitesFromSources.Select(s => s.NormilizeStringUrl()))
            .Take(size);
        return sites
            .Where(t => oldSites.Contains(t.WebsiteUrl.NormilizeStringUrl()))
            .Select(t => t.Id);
    }
}
