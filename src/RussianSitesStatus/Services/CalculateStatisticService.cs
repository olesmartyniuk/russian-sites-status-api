using Newtonsoft.Json;
using RussianSitesStatus.Database.Models;

namespace RussianSitesStatus.Services;

public class CalculateStatisticService
{
    private readonly ILogger<CalculateStatisticService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CalculateStatisticService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<CalculateStatisticService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task CreateStatisticAsync()
    {
        using (var serviceScope = _serviceScopeFactory.CreateScope())
        {
            var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();
            var oldestDateTime = await databaseStorage.GetOldestCheckSiteDateAsync();
            if (!oldestDateTime.HasValue)
            {
                _logger.LogInformation("There is no any checks to procces.");
                return;
            }

            var statisticDate = oldestDateTime.Value.Date;
            while (statisticDate.Date < DateTime.UtcNow.Date)
            {
                var siteIds = await databaseStorage.GetUniqueSiteIdsAsync(statisticDate);
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
