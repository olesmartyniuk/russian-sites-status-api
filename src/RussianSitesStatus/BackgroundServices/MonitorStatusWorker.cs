using Microsoft.Extensions.Options;
using RussianSitesStatus.Configuration;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.BackgroundServices;

public class MonitorStatusWorker : BackgroundService
{
    private readonly MonitorSitesConfiguration _monitorSitesConfiguration;

    private readonly MonitorSitesStatusService _monitorSitesService;
    private readonly ILogger<MonitorStatusWorker> _logger;
    public MonitorStatusWorker(
        ILogger<MonitorStatusWorker> logger,
        MonitorSitesStatusService syncSitesService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _monitorSitesService = syncSitesService;
        _monitorSitesConfiguration = serviceProvider
            .GetRequiredService<IOptions<MonitorSitesConfiguration>>().Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await Task.Delay(TimeSpan.FromSeconds(_monitorSitesConfiguration.WaitBeforeFirstIterationSeconds), stoppingToken);

        var spentTime = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                spentTime = await _monitorSitesService.MonitorAllAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            var waitToNextIteration = _monitorSitesConfiguration.WaitToNextCheckSeconds - spentTime;
            if (waitToNextIteration > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(waitToNextIteration), stoppingToken);
                _logger.LogInformation($"Monitoring takes: {spentTime} seconds.");
            }
            else
            {
                _logger.LogWarning($"Monitoring takes: {spentTime} seconds. It's more than one iteration({_monitorSitesConfiguration.WaitToNextCheckSeconds } seconds) should be.");
            }
        }
    }
}