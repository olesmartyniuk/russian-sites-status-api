using Microsoft.Extensions.Options;
using RussianSitesStatus.Configuration;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.BackgroundServices;

public class SyncSitesWorker : BackgroundService
{
    private readonly SyncSitesConfiguration _syncSitesConfiguration;

    private readonly IServiceScopeFactory _serviceFactory;
    private readonly ILogger<SyncSitesWorker> _logger;
    public SyncSitesWorker(
        ILogger<SyncSitesWorker> logger,
        IServiceProvider serviceProvider,
        IServiceScopeFactory serviceFactory)
    {
        _logger = logger;
        _syncSitesConfiguration = serviceProvider
            .GetRequiredService<IOptions<SyncSitesConfiguration>>().Value;
        _serviceFactory = serviceFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceFactory.CreateScope();
        var syncSitesService = serviceScope.ServiceProvider.GetRequiredService<ISyncSitesService>();

        await Task.Delay(TimeSpan.FromSeconds(_syncSitesConfiguration.WaitBeforeFirstIterationSeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await syncSitesService.SyncAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            await Task.Delay(TimeSpan.FromSeconds(_syncSitesConfiguration.WaitToNextCheckSeconds), stoppingToken);
        }
    }
}