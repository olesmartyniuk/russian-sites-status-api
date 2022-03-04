using Microsoft.Extensions.Options;
using RussianSitesStatus.Configuration;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.BackgroundServices;

public class SyncSitesWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncSitesWorker> _logger;
    public SyncSitesWorker(
        ILogger<SyncSitesWorker> logger,
        IServiceScopeFactory serviceFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceFactory = serviceFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sitesSyncInterval = int.Parse(_configuration["SITES_SYNC_INTERVAL"]);
        if (sitesSyncInterval <= 0)
        {
            return;
        }

        using var serviceScope = _serviceFactory.CreateScope();
        var syncSitesService = serviceScope.ServiceProvider.GetRequiredService<ISyncSitesService>();

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

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

            await Task.Delay(TimeSpan.FromSeconds(sitesSyncInterval), stoppingToken);
        }
    }
}