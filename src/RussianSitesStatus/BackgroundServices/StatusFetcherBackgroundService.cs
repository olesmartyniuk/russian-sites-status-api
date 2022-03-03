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
    private readonly IDataService _dataService;

    public StatusFetcherBackgroundService(
        Storage<SiteVM> liteStatusStorage,
        Storage<SiteDetailsVM> fullStatusStorage,
        ILogger<StatusFetcherBackgroundService> logger,
        IDataService dataService)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _logger = logger;
        _dataService = dataService;
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
        //TODOPavlo: Create a new implemantaion of IDataService, register it in DI container

        var sites = await _dataService.GetAllAsync();
        _liteStatusStorage.ReplaceAll(sites);

        _fullStatusStorage.ReplaceAll(await _dataService.GetAllSitesDetailsAsync(sites));
    }
}