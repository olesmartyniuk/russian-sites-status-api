using RussianSitesStatus.Services;

namespace RussianSitesStatus.BackgroundServices;

public class MonitorStatusWorker : BackgroundService
{
    private readonly ILogger<MonitorStatusWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IConfiguration _configuration;

    public MonitorStatusWorker(
        ILogger<MonitorStatusWorker> logger,
        IServiceScopeFactory serviceScopeFactory, 
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var siteCheckInterval = int.Parse(_configuration["SITE_CHECK_INTERVAL"]);
        if (siteCheckInterval <= 0)
        {
            return;
        }

        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        var spentTime = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var serviceScope = _serviceScopeFactory.CreateScope())
                {
                    var monitorSitesService = serviceScope.ServiceProvider.GetRequiredService<MonitorSitesStatusService>();

                    spentTime = await monitorSitesService.MonitorAllAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            var waitToNextIteration = siteCheckInterval - spentTime;
            if (waitToNextIteration > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(waitToNextIteration), stoppingToken);
                _logger.LogInformation($"Monitoring takes: {spentTime} seconds.");
            }
            else
            {
                _logger.LogWarning($"Monitoring takes: {spentTime} seconds. It's more than one iteration({ siteCheckInterval } seconds) should be.");
            }
        }
    }
}