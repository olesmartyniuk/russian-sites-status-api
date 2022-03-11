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
    private readonly int _maxSitesInQueue;
    private readonly int _sitesSkip;
    private readonly int _sitesTake;
    private readonly TimeSpan _reservedTimeForExecutionInMilliseconds;

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

        _maxSitesInQueue = int.Parse(_configuration["MAX_SITES_IN_QUEUE"]);
        _sitesSkip = int.Parse(_configuration["SITE_CHECK_SKIP_TAKE"].Split(",")[0]);
        _sitesTake = int.Parse(_configuration["SITE_CHECK_SKIP_TAKE"].Split(",")[1]);

        var monitorWorkerInterval = int.Parse(_configuration["SITE_CHECK_INTERVAL"]);
        _reservedTimeForExecutionInMilliseconds = TimeSpan.FromMilliseconds(TimeSpan.FromSeconds(monitorWorkerInterval - 20).TotalMilliseconds / _sitesTake);
    }

    public async Task<int> MonitorAllAsync()
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

        var timer = new Stopwatch();
        timer.Start();

        var sitesToCheck = await databaseStorage
            .GetSites(_sitesSkip, _sitesTake);
        var regionsToCheck = await databaseStorage
            .GetRegions(true);

        if (!IsValidForProcessing(sitesToCheck, regionsToCheck))
        {
            timer.Stop();
            return (int)timer.Elapsed.TotalSeconds;
        }

        var checkedAt = DateTime.UtcNow;

        var tasks = new List<Task>();
        foreach (var region in regionsToCheck)
        {
            tasks.Add(Task.Run(async () => await CheckSitesForRegion(region, sitesToCheck, checkedAt)));
        }

        await Task.WhenAll(tasks);
        await UpdateCheckedAt(checkedAt, sitesToCheck);

        timer.Stop();
        return (int)timer.Elapsed.TotalSeconds;
    }

    private async Task CheckSitesForRegion(Region region, IEnumerable<Site> sites, DateTime checkedAt)
    {
        var checks = new ConcurrentBag<Check>();
        var throttler = new SemaphoreSlim(_maxSitesInQueue, _maxSitesInQueue);
        var totalStartedTasks = 0;
        var stopwatch = Stopwatch.StartNew();
        var completionTimes = new ConcurrentQueue<TimeSpan>();

        var tasks = new List<Task>();
        foreach (var site in sites)
        {
            var task = Task.Run(async () =>
            {
                Debug.WriteLine($"Task {Task.CurrentId} begins and waits for the semaphore.Number = {throttler.CurrentCount}");
                await throttler.WaitAsync();
                if (Interlocked.Increment(ref totalStartedTasks) > _maxSitesInQueue)
                {
                    completionTimes.TryDequeue(out var earliest);
                    var elapsed = stopwatch.Elapsed - earliest;
                    var delay = _reservedTimeForExecutionInMilliseconds - elapsed;
                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(_reservedTimeForExecutionInMilliseconds);
                    }
                }

                Debug.WriteLine($"Task {Task.CurrentId} enters the semaphore.Number = {throttler.CurrentCount}");
                try
                {
                    var check = await _checkSiteService.Check(site, region, checkedAt);
                    checks.Add(check);
                }
                finally
                {
                    var semaphoreCount = throttler.Release();
                    completionTimes.Enqueue(stopwatch.Elapsed);
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

    private async Task UpdateCheckedAt(DateTime checkedAt, IEnumerable<Site> sitesToCheck)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();

        var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

        await databaseStorage.UpdateCheckedAt(sitesToCheck, checkedAt);
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
