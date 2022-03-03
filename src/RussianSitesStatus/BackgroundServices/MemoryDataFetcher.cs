using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.BackgroundServices;

public class MemoryDataFetcher : BackgroundService
{
    private const int WAIT_TO_NEXT_CHECK_SECONDS = 30;

    private readonly InMemoryStorage<SiteVM> _liteStatusStorage;
    private readonly InMemoryStorage<SiteDetailsVM> _fullStatusStorage;
    private readonly BaseInMemoryStorage<Region> _regionStorage;
    private readonly ILogger<MemoryDataFetcher> _logger;
    private readonly IFetchDataService _fetchDataService;

    public MemoryDataFetcher(
        InMemoryStorage<SiteVM> liteStatusStorage,
        InMemoryStorage<SiteDetailsVM> fullStatusStorage,
        BaseInMemoryStorage<Region> regionStorage,
        ILogger<MemoryDataFetcher> logger,
        IFetchDataService dataService
       )
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _logger = logger;
        _fetchDataService = dataService;
        _regionStorage = regionStorage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RaplaceInMemoryStorage();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            await Task.Delay(TimeSpan.FromSeconds(WAIT_TO_NEXT_CHECK_SECONDS), stoppingToken);
        }
    }

    private async Task RaplaceInMemoryStorage()
    {
        //TODOPavlo: Create a new implemantaion of IFetchDataService, register it in DI container
        var sites = await _fetchDataService.GetAllSitesDetailsAsync();
        _liteStatusStorage.ReplaceAll(sites.Select(vm => vm as SiteVM));
        _fullStatusStorage.ReplaceAll(sites);
    }
}