using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RussianSitesStatus.Services;

public class MonitorSitesStatusService
{
    private static int MAX_SITES_IN_QUEUE = 100;
    
    private readonly InMemoryStorage<SiteVM> _liteInMemorySiteStorage;
    private readonly BaseInMemoryStorage<RegionVM> _inMemoryRegionStorage;
    private readonly ICheckSiteService _checkSiteService;
    private readonly ILogger<MonitorSitesStatusService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MonitorSitesStatusService(
        BaseInMemoryStorage<RegionVM> inMemoryRegionStorage,
        InMemoryStorage<SiteVM> liteInMemorySiteStorage,
        ICheckSiteService checkSiteService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MonitorSitesStatusService> logger)
    {
        _liteInMemorySiteStorage = liteInMemorySiteStorage;
        _checkSiteService = checkSiteService;
        _inMemoryRegionStorage = inMemoryRegionStorage;
        _serviceScopeFactory = serviceScopeFactory;        
        _logger = logger;
    }

    public async Task<int> MonitorAllAsync()
    {
        var timer = new Stopwatch();
        timer.Start();

        var allSites = _liteInMemorySiteStorage
            .GetAll();

        var allRegions = _inMemoryRegionStorage
            .GetAll()
            .Where(r => r.ProxyIsActive);        

        if (!IsValidForProcessing(allSites, allRegions))
        {
            timer.Stop();
            return (int)timer.Elapsed.TotalSeconds;
        }

        var iteration = Guid.NewGuid();
        var tasks = allRegions
            .Select(region => CheckSitesForRegion(region, allSites, iteration));

        await Task.WhenAll(tasks);

        timer.Stop();
        return (int)timer.Elapsed.TotalSeconds;
    }

    private async Task CheckSitesForRegion(RegionVM region, IEnumerable<SiteVM> sites, Guid iteration)
    {
        var checks = new ConcurrentBag<Check>();

        var throttler = new SemaphoreSlim(initialCount: MAX_SITES_IN_QUEUE);
        var tasks = sites.Select(async site =>
        {
            await throttler.WaitAsync();
            try
            {
                var check = await _checkSiteService.CheckAsync(site, region, iteration);
                checks.Add(check);    
            }
            finally
            {
                throttler.Release();
            }
        });
        
        await Task.WhenAll(tasks);

        await SaveChecks(checks);
    }

    private async Task SaveChecks(ConcurrentBag<Check> checks)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();
        
        var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

        await databaseStorage.AddChecksAsync(checks.ToList());
    }

    private bool IsValidForProcessing(IEnumerable<SiteVM> allSites, IEnumerable<RegionVM> allRegions)
    {
        if (!allSites.Any() || !allRegions.Any())
        {
            _logger.LogWarning($"There is nothing to monitor. Regions number = {allRegions.Count()}, Sites number = {allSites.Count()}");
            return false;
        }

        return true;
    }
}
