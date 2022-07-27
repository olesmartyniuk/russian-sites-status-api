namespace RussianSitesStatus.Services;

public class ArchiveService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ArchiveService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task ArchiveOldData()
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();
        var databaseStorage = serviceScope.ServiceProvider
            .GetRequiredService<DatabaseStorage>();

        var minCheckedAt = await databaseStorage.GetMinCheckedAt();
        var maxStatisticsDay = await databaseStorage.GetMaxStatistisDay();
        if (!minCheckedAt.HasValue || !maxStatisticsDay.HasValue)
        {
            return;
        }

        var minCheckedAtDate = minCheckedAt.Value.Date;
        var maxStatisticsDayDate = maxStatisticsDay.Value.Date;
        if (minCheckedAtDate < maxStatisticsDayDate)
        {
            await databaseStorage.DeleteChecksBefore(maxStatisticsDayDate);
        }
    }
}
