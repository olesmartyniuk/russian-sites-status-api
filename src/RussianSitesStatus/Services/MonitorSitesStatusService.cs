using Microsoft.Extensions.Options;
using RussianSitesStatus.Configuration;
using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace RussianSitesStatus.Services;
public class MonitorSitesStatusService
{
    private static int ITERATION_NUMBER = 10;
    private readonly int _rateInMilliseconds;

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
        IOptions<MonitorSitesConfiguration> configurations,
        ILogger<MonitorSitesStatusService> logger)
    {

        _liteInMemorySiteStorage = liteInMemorySiteStorage;
        _checkSiteService = checkSiteService;
        _inMemoryRegionStorage = inMemoryRegionStorage;
        _serviceScopeFactory = serviceScopeFactory;
        _rateInMilliseconds = (int)TimeSpan.FromSeconds(configurations.Value.Rate).TotalMilliseconds;
        _logger = logger;
    }

    public async Task<int> MonitorAllAsync()
    {
        var timer = new Stopwatch();
        timer.Start();

        var allSites = _liteInMemorySiteStorage.GetAll();
        var allRegions = _inMemoryRegionStorage.GetAll();
        if (IsValidForProcessing(allSites, allRegions))
        {
            timer.Stop();
            return (int)timer.Elapsed.TotalSeconds;
        }

        var taskList = new List<Task>();
        foreach (var batch in allSites.Chunk(allSites.Count() / ITERATION_NUMBER))
        {
            taskList.Add(Task.Run(async () => await CheckSitesOnAllRegionsAsync(batch, allRegions)));

            //await Task.Delay(_rateInMilliseconds / ITERATION_NUMBER);
            //await Task.Delay(100);
        }

        await Task.WhenAll(taskList);

        timer.Stop();
        return (int)timer.Elapsed.TotalSeconds;
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

    private async Task CheckSitesOnAllRegionsAsync(IEnumerable<SiteVM> sites, IEnumerable<RegionVM> allRegions)
    {
        var checks = new ConcurrentBag<Check>();
        var taskList = new List<Task>();
        using (var serviceScope = _serviceScopeFactory.CreateScope())
        {
            var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();
            foreach (var item in sites)
            {
                taskList.Add(Task.Run(async () => await CheckOneSiteOnAllRegionsAsync(item, allRegions, checks)));
            }

            await Task.WhenAll(taskList);
            await databaseStorage.AddChecksAsync(checks.ToList());
        }
    }

    private async Task CheckOneSiteOnAllRegionsAsync(SiteVM site, IEnumerable<RegionVM> allRegions, ConcurrentBag<Check> checks)
    {
        foreach (var region in allRegions)
        {
            var check = await _checkSiteService.CheckAsync(site, region);
            checks.Add(check);
        }
    }
}
