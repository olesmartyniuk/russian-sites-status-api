using Newtonsoft.Json;
using RussianSitesStatus.Database.Models;
using System.Diagnostics;

namespace RussianSitesStatus.Services;

public class ArchiveStatisticService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ArchiveStatisticService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> ArchiveStatistic()
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();
        var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();
        var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<ArchiveStatisticService>>();

        try
        {
            var timer = new Stopwatch();
            timer.Start();

            await ExecuteArchiveStatistic(databaseStorage, logger);

            timer.Stop();
            var spentTime = (int)timer.Elapsed.TotalSeconds;
            logger.LogInformation($"{nameof(ArchiveStatisticService)}: Archivation executed in {spentTime} seconds.");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(ArchiveStatisticService)}: Unhandled exception while archiving statistic: {ex}");
            return false;
        }                
    }

    public async Task ExecuteArchiveStatistic(DatabaseStorage databaseStorage, ILogger<ArchiveStatisticService> logger)
    {
        var sitesWithDate = await databaseStorage
            .GetSitesWithDateToAgregateStat();

        if (sitesWithDate.Count() == 0)
        {
            logger.LogInformation($"{nameof(ArchiveStatisticService)}: There is no any checks to archive.");
            return;
        }

        foreach (var siteWithDate in sitesWithDate)
        {
            try
            {
                var statistics = await databaseStorage
                    .CalculateStatistic(
                        siteWithDate.SiteId,
                        siteWithDate.AgregateFor);

                var newCheck = new ChecksStatistics
                {
                    SiteId = siteWithDate.SiteId,
                    Day = siteWithDate.AgregateFor,
                    Data = JsonConvert.SerializeObject(statistics)
                };

                await databaseStorage.AddChecksStatistics(newCheck);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"{nameof(ArchiveStatisticService)}: SiteId={siteWithDate.SiteId}, AgregateFor={siteWithDate.AgregateFor}. Error: {ex}"); ;
            }
        }

        var uniqSitesCount = sitesWithDate
            .Select(s => s.SiteId)
            .Distinct()
            .Count();
        var uniqDates = sitesWithDate
            .Select(s => s.AgregateFor.ToString("MM-dd"))
            .Distinct();
        
        logger.LogInformation($"{nameof(ArchiveStatisticService)}: Processed {uniqSitesCount} sites for days [{string.Join(",", uniqDates)}]");
    }
}
