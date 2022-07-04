using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RussianSitesStatus.Services;

public class ArchiveService
{
    private readonly ILogger<MonitorSitesStatusService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ArchiveService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MonitorSitesStatusService> logger,
        IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task ArchiveOldData()
    {
        using (var serviceScope = _serviceScopeFactory.CreateScope())
        {
            var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

            var oldestChecksDateTime = await databaseStorage.GetOldestCheckSiteDate();
            var lastStatisticsDateTime = await databaseStorage.GetNewestStatistisDate(); //TODOVK: check if statistis exist per site
            if (!oldestChecksDateTime.HasValue || !lastStatisticsDateTime.HasValue)
            {
                return;
            }

            var oldestChecksDate = oldestChecksDateTime.Value.Date;
            var lastStatisticsDate = lastStatisticsDateTime.Value.Date;
            if (oldestChecksDate < lastStatisticsDate)
            {
                await databaseStorage.DeleteStatistis(lastStatisticsDate);
            }
        }
    }
}
