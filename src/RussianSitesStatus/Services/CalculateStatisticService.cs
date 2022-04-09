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
        using var serviceScope = _serviceScopeFactory.CreateScope();
        var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

        var sitesWithDate = await databaseStorage
            .GetSitesWithDateToAgregateStat();

        if (sitesWithDate.Count() == 0)
        {
            _logger.LogInformation("There is no any checks to procces.");
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
                _logger.LogError(ex, $"SiteId={siteWithDate.SiteId}, AgregateFor={siteWithDate.AgregateFor}"); ;
            }
        }

        var uniqSitesCount = sitesWithDate
            .Select(s => s.SiteId)
            .Distinct()
            .Count();
        var uniqDates = sitesWithDate
            .Select(s => s.AgregateFor.ToString("MM-dd"))
            .Distinct();
        _logger.LogInformation($"Processed {uniqSitesCount} sites for days [{string.Join(",", uniqDates)}]");
    }
}
