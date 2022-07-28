using RussianSitesStatus.Extensions;
using RussianSitesStatus.Services;
using System.Globalization;

namespace RussianSitesStatus.BackgroundServices;

public class ArchiveStatisticWorker : BackgroundService
{
    private readonly ILogger<MonitorStatusWorker> _logger;
    private readonly ArchiveStatisticService _calculateStatisticService;
    private readonly IConfiguration _configuration;

    public ArchiveStatisticWorker(
        ILogger<MonitorStatusWorker> logger,
        ArchiveStatisticService calculateStatisticsService,
        IConfiguration configuration)
    {
        _logger = logger;
        _calculateStatisticService = calculateStatisticsService;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!TimeSpan.TryParseExact(_configuration["CALCULATE_STATISTICS_AT"], "hh':'mm':'ss", CultureInfo.CurrentCulture, out TimeSpan calculateAt))
        {
            return;
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(calculateAt.WaitTimeSpan(), stoppingToken);
            try
            {
                await _calculateStatisticService.ArchiveStatistic();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ArchiveStatisticWorker)}: Unhandled exception while fetching statuses");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}