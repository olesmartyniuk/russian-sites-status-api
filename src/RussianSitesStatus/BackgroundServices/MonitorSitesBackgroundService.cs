using Microsoft.Extensions.Options;
using RussianSitesStatus.Configuration;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.BackgroundServices;

public class MonitorSitesBackgroundService : BackgroundService
{
    private readonly MonitorSitesConfiguration _syncSitesConfiguration;

    private readonly MonitorSitesService _monitorSitesService;
    private readonly ILogger<MonitorSitesBackgroundService> _logger;
    public MonitorSitesBackgroundService(
        ILogger<MonitorSitesBackgroundService> logger,
        MonitorSitesService syncSitesService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _monitorSitesService = syncSitesService;
        _syncSitesConfiguration = serviceProvider
            .GetRequiredService<IOptions<MonitorSitesConfiguration>>().Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(_syncSitesConfiguration.WaitBeforeFirstIterationSeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _monitorSitesService.MonitorAllAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            await Task.Delay(TimeSpan.FromSeconds(_syncSitesConfiguration.WaitToNextCheckSeconds), stoppingToken);
        }
    }
}