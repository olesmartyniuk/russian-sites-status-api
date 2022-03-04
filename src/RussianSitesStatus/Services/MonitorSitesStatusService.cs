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

    private ConcurrentBag<Check> Checks = new ConcurrentBag<Check>();

    private readonly InMemoryStorage<SiteVM> _liteInMemorySiteStorage;
    private readonly BaseInMemoryStorage<RegionVM> _inMemoryRegionStorage;
    private readonly ICheckSiteService _checkSiteService;
    private readonly ILogger<MonitorSitesStatusService> _logger;
    private readonly DatabaseStorage _databaseStorage;

    public MonitorSitesStatusService(
        BaseInMemoryStorage<RegionVM> inMemoryRegionStorage,
        InMemoryStorage<SiteVM> liteInMemorySiteStorage,
        ICheckSiteService checkSiteService,
        IOptions<MonitorSitesConfiguration> configurations,
        ILogger<MonitorSitesStatusService> logger
        , DatabaseStorage databaseStorage)
    {

        _liteInMemorySiteStorage = liteInMemorySiteStorage;
        _checkSiteService = checkSiteService;
        _inMemoryRegionStorage = inMemoryRegionStorage;
        _rateInMilliseconds = (int)TimeSpan.FromSeconds(configurations.Value.Rate).TotalMilliseconds;
        _logger = logger;
        _databaseStorage = databaseStorage;
    }

    public async Task<int> MonitorAllAsync()
    {
        var timer = new Stopwatch();
        timer.Start();

        var allSites = _liteInMemorySiteStorage.GetAll().Take(10);
        var allRegions = _inMemoryRegionStorage.GetAll().Take(3);
        if (!allSites.Any() || !allRegions.Any())
        {
            _logger.LogWarning($"There is nothing to monitor. Regions number = {allRegions.Count()}, Sites number = {allSites.Count()}");
            timer.Stop();
            return timer.Elapsed.Seconds;
        }

        var taskList = new List<Task>();
        foreach (var batch in allSites.Chunk(allSites.Count() / ITERATION_NUMBER))
        {
            foreach (var item in batch)
            {
                taskList.Add(Task.Run(() => CheckOneSiteOnAllRegionsAsync(item, allRegions, Checks)));
            }

            //await Task.Delay(_rateInMilliseconds / ITERATION_NUMBER);
            //await Task.Delay(100);
        }

        await Task.WhenAll(taskList);

        timer.Stop();

        await _databaseStorage.AddChecks(Checks.ToList());

        return (int)timer.Elapsed.TotalSeconds;
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
