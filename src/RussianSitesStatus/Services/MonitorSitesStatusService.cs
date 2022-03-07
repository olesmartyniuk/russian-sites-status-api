using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RussianSitesStatus.Services;

public class MonitorSitesStatusService
{
    private readonly ICheckSiteService _checkSiteService;
    private readonly ILogger<MonitorSitesStatusService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private IConfiguration _configuration;

    public MonitorSitesStatusService(                
        ICheckSiteService checkSiteService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MonitorSitesStatusService> logger,
        IConfiguration configuration)
    {        
        _checkSiteService = checkSiteService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<int> MonitorAllAsync()
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

        var timer = new Stopwatch();
        timer.Start();

        var allSites = await databaseStorage
            .GetAllSites();
        var allRegions = await databaseStorage
            .GetRegions(true);

        if (!IsValidForProcessing(allSites, allRegions))
        {
            timer.Stop();
            return (int)timer.Elapsed.TotalSeconds;
        }

        var checkedAt = DateTime.UtcNow;

        var tasks = new List<Task>();
        foreach (var region in allRegions)
        {
            tasks.Add(Task.Run(async () => await CheckSitesForRegion(region, allSites, checkedAt)));
        }

        await Task.WhenAll(tasks);
        await UpdateCheckedAt(checkedAt);

        timer.Stop();
        return (int)timer.Elapsed.TotalSeconds;
    }

    private async Task CheckSitesForRegion(Region region, IEnumerable<Site> sites, DateTime checkedAt)
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
                    var check = await _checkSiteService.Check(site, region, checkedAt);
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

    private async Task UpdateCheckedAt(DateTime checkedAt)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

        await databaseStorage.UpdateCheckedAt(checkedAt);
    }

    private bool IsValidForProcessing(IEnumerable<Site> allSites, IEnumerable<Region> allRegions)
    {
        if (!allSites.Any() || !allRegions.Any())
        {
            _logger.LogWarning($"There is nothing to monitor. Regions number = {allRegions.Count()}, Sites number = {allSites.Count()}");
            return false;
        }

        return true;
    }
}
