using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RussianSitesStatus.Services;

public class MonitorSitesStatusService
{
    private readonly InMemoryStorage<SiteVM> _liteInMemorySiteStorage;
    private readonly BaseInMemoryStorage<RegionVM> _inMemoryRegionStorage;
    private readonly ICheckSiteService _checkSiteService;
    private readonly ILogger<MonitorSitesStatusService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IConfiguration _configuration;

    public MonitorSitesStatusService(
        BaseInMemoryStorage<RegionVM> inMemoryRegionStorage,
        InMemoryStorage<SiteVM> liteInMemorySiteStorage,
        ICheckSiteService checkSiteService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MonitorSitesStatusService> logger,
        IConfiguration configuration)
    {
        _liteInMemorySiteStorage = liteInMemorySiteStorage;
        _checkSiteService = checkSiteService;
        _inMemoryRegionStorage = inMemoryRegionStorage;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configuration = configuration;
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

        var tasks = new List<Task>();
        foreach (var region in allRegions)
        {
            tasks.Add(Task.Run(async () => await CheckSitesForRegion(region, allSites, iteration)));
        }

        await Task.WhenAll(tasks);

        timer.Stop();
        return (int)timer.Elapsed.TotalSeconds;
    }

    private async Task CheckSitesForRegion(RegionVM region, IEnumerable<SiteVM> sites, Guid iteration)
    {
        var checks = new ConcurrentBag<Check>();

        var maxSitesInQueue = int.Parse(_configuration["MAX_SITES_IN_QUEUE"]);
        var throttler = new SemaphoreSlim(maxSitesInQueue, maxSitesInQueue);

        var tasks = new List<Task>();
        foreach (var site in sites)
        {
            var task = Task.Run(async () =>
            {
                Debug.WriteLine($"Task {Task.CurrentId} begins and waits for the semaphore.Number = {throttler.CurrentCount}");
                await throttler.WaitAsync();
                Debug.WriteLine($"Task {Task.CurrentId} enters the semaphore.Number = {throttler.CurrentCount}");
                try
                {
                    //await Task.Delay(2000);
                    var check = await _checkSiteService.CheckAsync(site, region, iteration);
                    checks.Add(check);
                }
                finally
                {
                    var semaphoreCount = throttler.Release();
                    Debug.WriteLine($"Task {Task.CurrentId} releases the semaphore; previous count: NumberRelease = {semaphoreCount}.NumberCurrentCount = {throttler.CurrentCount}");
                }
            });

            tasks.Add(task);
        }

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
