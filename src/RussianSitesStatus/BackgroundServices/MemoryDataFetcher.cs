using RussianSitesStatus.Models;
using RussianSitesStatus.Services;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.BackgroundServices;

public class MemoryDataFetcher : BackgroundService
{
    private readonly InMemoryStorage<Site> _liteStatusStorage;
    private readonly InMemoryStorage<SiteDetails> _fullStatusStorage;
    private readonly BaseInMemoryStorage<Region> _regionStorage;
    private readonly ILogger<MemoryDataFetcher> _logger;
    private readonly IFetchDataService _fetchDataService;
    private readonly IConfiguration _configuration;
    private readonly int _memoryDataSyncInterval;

    public MemoryDataFetcher(
        InMemoryStorage<Site> liteStatusStorage,
        InMemoryStorage<SiteDetails> fullStatusStorage,
        BaseInMemoryStorage<Region> regionStorage,
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

        _memoryDataSyncInterval = int.Parse(_configuration["MEMORY_DATA_SYNC_INTERVAL"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {        
        if (_memoryDataSyncInterval <= 0)
        {
            _logger.LogInformation($"MemoryDataFetcher will not start, MEMORY_DATA_SYNC_INTERVAL={_memoryDataSyncInterval}.");
            return;
        }

        _logger.LogInformation($"MemoryDataFetcher started, MEMORY_DATA_SYNC_INTERVAL={_memoryDataSyncInterval}.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RaplaceInMemoryStorage();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while synchronized data from DB.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_memoryDataSyncInterval), stoppingToken);
        }
    }

    private async Task RaplaceInMemoryStorage()
    {        
        var sites = await _fetchDataService.GetAllSitesDetailsAsync();
        _logger.LogInformation($"Fetched {sites.Count()} sites.");

        _fullStatusStorage.ReplaceAll(sites);
        _liteStatusStorage.ReplaceAll(sites.Select(vm => vm as Site));

        var regions = await _fetchDataService.GetAllRegionsAsync();
        _regionStorage.ReplaceAll(regions);

        _logger.LogInformation($"All in-memory storages were updated.");
    }
}