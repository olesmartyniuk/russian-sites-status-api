using RussianSitesStatus.Extensions;
using RussianSitesStatus.Services;
using System.Diagnostics;
using System.Globalization;

namespace RussianSitesStatus.BackgroundServices;

public class CalcualteStatisticWorker : BackgroundService
{
    private readonly ILogger<MonitorStatusWorker> _logger;
    private readonly CalculateStatisticService _calculateStatisticService;
    private IConfiguration _configuration;

    public CalcualteStatisticWorker(
        ILogger<MonitorStatusWorker> logger,
        CalculateStatisticService calculateStatisticsService,
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

        var spentTime = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(calculateAt.WaitTimeSpan(), stoppingToken);
            try
            {
                var timer = new Stopwatch();
                timer.Start();

                await _calculateStatisticService.SaveStatistic();

                timer.Stop();
                spentTime = (int)timer.Elapsed.TotalSeconds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while fetching statuses");
            }
            _logger.LogInformation($"{nameof(CalcualteStatisticWorker)}: executed iteration in {spentTime} seconds.");

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}