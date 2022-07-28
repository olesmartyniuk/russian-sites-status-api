using System.Diagnostics;

namespace RussianSitesStatus.Services;

public class CleanupChecksService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CleanupChecksService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> ArchiveOldData()
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var logger = serviceScope.ServiceProvider
            .GetRequiredService<ILogger<CleanupChecksService>>();
        var databaseStorage = serviceScope.ServiceProvider
            .GetRequiredService<DatabaseStorage>();

        try
        {
            var timer = new Stopwatch();
            timer.Start();

            await ExecuteCleanup(logger, databaseStorage);

            timer.Stop();

            var spentTime = (int)timer.Elapsed.TotalSeconds;

            logger.LogInformation($"{nameof(CleanupChecksService)}: Cleanup successfully executed in {spentTime} seconds.");

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{nameof(CleanupChecksService)}: Unhandled exception during cleanup");

            return false;
        }
    }

    private async Task ExecuteCleanup(ILogger<CleanupChecksService> logger, DatabaseStorage databaseStorage)
    {
        var minCheckedAt = await databaseStorage.GetMinCheckedAt();
        var maxStatisticsDay = await databaseStorage.GetMaxStatistisDay();
        if (!minCheckedAt.HasValue || !maxStatisticsDay.HasValue)
        {
            return;
        }

        var minCheckedAtDate = minCheckedAt.Value.Date;
        var maxStatisticsDayDate = maxStatisticsDay.Value.Date;
        logger.LogInformation($"{nameof(CleanupChecksService)}: Oldest checked was performed at '{minCheckedAtDate}'.");
        logger.LogInformation($"{nameof(CleanupChecksService)}: Last statistic collected for '{maxStatisticsDayDate}'.");

        if (minCheckedAtDate < maxStatisticsDayDate)
        {
            await databaseStorage.DeleteChecksBefore(maxStatisticsDayDate);
            logger.LogInformation($"{nameof(CleanupChecksService)}: All checks before '{maxStatisticsDayDate}' were removed.");
        }
        else
        {
            logger.LogInformation($"{nameof(CleanupChecksService)}: There are no checks that can be removed.");
        }
    }
}
