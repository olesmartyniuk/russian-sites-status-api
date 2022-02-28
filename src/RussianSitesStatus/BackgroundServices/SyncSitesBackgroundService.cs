using RussianSitesStatus.Services;

namespace RussianSitesStatus.BackgroundServices;

public class SyncSitesBackgroundService : BackgroundService
{
    private const int WAIT_TO_NEXT_CHECK_SECONDS = 25;
    private const int WAIT_BEFORE_THE_FIRST_ITARATION_SECONDS = 20;

    private readonly SyncSitesService _syncSitesService;
    private readonly ILogger<StatusFetcherBackgroundService> _logger;
    public SyncSitesBackgroundService(ILogger<StatusFetcherBackgroundService> logger, SyncSitesService syncSitesService)
    {
        _logger = logger;
        _syncSitesService = syncSitesService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(WAIT_BEFORE_THE_FIRST_ITARATION_SECONDS), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _syncSitesService.SyncAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            await Task.Delay(TimeSpan.FromSeconds(WAIT_TO_NEXT_CHECK_SECONDS), stoppingToken);
        }
    }
}