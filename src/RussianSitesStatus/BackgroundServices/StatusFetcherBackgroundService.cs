using RussianSitesStatus.Models;
using RussianSitesStatus.Services;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.BackgroundServices;

public class StatusFetcherBackgroundService : BackgroundService
{
    private const int WAIT_TO_NEXT_CHECK_SECONDS = 30;

    private readonly Storage<SiteVM> _liteStatusStorage;
    private readonly Storage<SiteDetailsVM> _fullStatusStorage;
    private readonly ILogger<StatusFetcherBackgroundService> _logger;
    private readonly IFetchDataService _fetchDataService;

    public StatusFetcherBackgroundService(
        Storage<SiteVM> liteStatusStorage,
        Storage<SiteDetailsVM> fullStatusStorage,
        ILogger<StatusFetcherBackgroundService> logger,
        IFetchDataService dataService)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _logger = logger;
        _fetchDataService = dataService;
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

        var sites = await _fetchDataService.GetAllAsync();
        _liteStatusStorage.ReplaceAll(sites);

        _fullStatusStorage.ReplaceAll(await _fetchDataService.GetAllSitesDetailsAsync(sites));
    }
}