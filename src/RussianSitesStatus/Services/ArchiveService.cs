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

    public async Task ArchiveOldDataAsync()
    {
        using (var serviceScope = _serviceScopeFactory.CreateScope())
        {
            var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

            var lastStatisticsDate = await databaseStorage.GetNewestStatistisDateAsync(); //TODOVK: check if statistis exist per site
            if (lastStatisticsDate.Date < DateTime.UtcNow.Date)
            {
                await databaseStorage.DeleteStatistisAsync(lastStatisticsDate.Date.AddDays(-20));
            }
            else
            {
                await databaseStorage.DeleteStatistisAsync(DateTime.UtcNow.Date.AddDays(-20));
            }
        }
    }
}
