using Newtonsoft.Json;
using RussianSitesStatus.Database.Models;

namespace RussianSitesStatus.Services;

public class CalculateStatisticsService
{
    private readonly ILogger<MonitorSitesStatusService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CalculateStatisticsService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MonitorSitesStatusService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task CreateStatisticsAsync()
    {
        using (var serviceScope = _serviceScopeFactory.CreateScope())
        {
            var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();
            var oldestDate = (await databaseStorage.GetOldestCheckSiteDate()).Date;

            var siteIds = await databaseStorage.GetUniqueSiteIdsAsync();

            var statisticDate = oldestDate.AddDays(1);
            while (statisticDate.Date < DateTime.UtcNow.Date)
            {
                foreach (var siteId in siteIds)
                {
                    try
                    {
                        if (await databaseStorage.HasStatisticsAsync(siteId, statisticDate))
                        {
                            continue;
                        }

                        var statistics = await databaseStorage.CalculateStatisticAsync(siteId, statisticDate);

                        var newChec = new ChecksStatistics
                        {
                            SiteId = siteId,
                            Day = statisticDate,
                            Data = JsonConvert.SerializeObject(statistics)
                        };

                        await databaseStorage.AddChecksStatisticsAsync(newChec);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Siteid = {siteId}"); ;
                    }
                }

                statisticDate = statisticDate.AddDays(1);
            }
        }
    }
}
