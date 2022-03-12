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
    private readonly int _monitorWorkerInterval;
    private readonly int _reservedTimeForExecution;

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

        _monitorWorkerInterval = int.Parse(_configuration["SITE_CHECK_INTERVAL"]);
        _reservedTimeForExecution =
            (int)TimeSpan.FromMilliseconds(TimeSpan.FromSeconds(_monitorWorkerInterval - 20).TotalMilliseconds /
            Math.Max(_sitesTake / _maxSitesInQueue, 1)).TotalSeconds;
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
        var completionTimes = new ConcurrentQueue<int>();

        _logger.LogInformation($"Check sites for region {region.Code} started at {DateTime.UtcNow}, should be finished at {DateTime.UtcNow.AddSeconds(_monitorWorkerInterval)}");

        var tasks = new List<Task>();
        foreach (var site in sites)
        {
            var task = Task.Run(async () =>
            {
                await throttler.WaitAsync();
                if (Interlocked.Increment(ref totalStartedTasks) > _maxSitesInQueue)
                {
                    completionTimes.TryDequeue(out var earliest);
                    var delay = _reservedTimeForExecution - earliest;
                    if (delay > 0)
                    {
                        _logger.LogInformation($"{site.Name} delay = {delay} seconds");
                        await Task.Delay(TimeSpan.FromSeconds(delay));
                    }
                }

                _logger.LogDebug($"{site.Name} check for region {region.Code} stared.");
                Check check = null;
                try
                {
                    check = await _checkSiteService.Check(site, region, checkedAt);
                    checks.Add(check);
                    _logger.LogDebug($"{site.Name} check for region {region.Code} finished.");
                }
                finally
                {
                    completionTimes.Enqueue(check?.SpentTime ?? _reservedTimeForExecution);
                    var semaphoreCount = throttler.Release();
                }
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        _logger.LogDebug($"Check sites for region {region.Code} at {DateTime.UtcNow}, total time: {stopwatch.ElapsedMilliseconds / 1000} sec");

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
