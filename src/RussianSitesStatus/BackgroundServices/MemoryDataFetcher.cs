using RussianSitesStatus.Models;
using RussianSitesStatus.Services;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.BackgroundServices;

public class MemoryDataFetcher : BackgroundService
{
    private readonly InMemoryStorage<SiteVM> _liteStatusStorage;
    private readonly InMemoryStorage<SiteDetailsVM> _fullStatusStorage;
    private readonly BaseInMemoryStorage<RegionVM> _regionStorage;
    private readonly ILogger<MemoryDataFetcher> _logger;
    private readonly IFetchDataService _fetchDataService;
    private IConfiguration _configuration;

    public MemoryDataFetcher(
        InMemoryStorage<SiteVM> liteStatusStorage,
        InMemoryStorage<SiteDetailsVM> fullStatusStorage,
        BaseInMemoryStorage<RegionVM> regionStorage,
        ILogger<MemoryDataFetcher> logger,
        IFetchDataService dataService,
        IConfiguration configuration
       )
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _logger = logger;
        _fetchDataService = dataService;
        _regionStorage = regionStorage;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var siteCheckInterval = int.Parse(_configuration["MEMORY_DATA_SYNC_INTERVAL"]);
        if (siteCheckInterval <= 0)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RaplaceInMemoryStorage();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while synchronized data from DB");
            }

            await Task.Delay(TimeSpan.FromSeconds(siteCheckInterval), stoppingToken);
        }
    }

    private async Task RaplaceInMemoryStorage()
    {        
        var sites = await _fetchDataService.GetAllSitesDetailsAsync();
     
        _fullStatusStorage.ReplaceAll(sites);
        _liteStatusStorage.ReplaceAll(sites.Select(vm => vm as SiteVM));

        var regions = await _fetchDataService.GetAllRegionsAsync();
        _regionStorage.ReplaceAll(regions);
    }
}