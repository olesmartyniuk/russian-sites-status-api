using Microsoft.Extensions.Options;
using RussianSitesStatus.Configuration;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.BackgroundServices;

public class SyncSitesBackgroundService : BackgroundService
{
    private readonly SyncSitesConfiguration _syncSitesConfiguration;

    private readonly ISyncSitesService _syncSitesService;
    private readonly ILogger<SyncSitesBackgroundService> _logger;
    public SyncSitesBackgroundService(
        ILogger<SyncSitesBackgroundService> logger,
        ISyncSitesService syncSitesService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _syncSitesService = syncSitesService;
        _syncSitesConfiguration = serviceProvider
            .GetRequiredService<IOptions<SyncSitesConfiguration>>().Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(_syncSitesConfiguration.WaitBeforeFirstIterationSeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //TODOPavlo:, TODOKhrystyna: Create a new implemantaion of ISyncSitesService, register it in DI container
                await _syncSitesService.SyncAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            await Task.Delay(TimeSpan.FromSeconds(_syncSitesConfiguration.WaitToNextCheckSeconds), stoppingToken);
        }
    }
}