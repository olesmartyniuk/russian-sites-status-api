using Microsoft.Extensions.Options;
using RussianSitesStatus.Configuration;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;
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

    public MonitorSitesStatusService(
         BaseInMemoryStorage<RegionVM> inMemoryRegionStorage,
        InMemoryStorage<SiteVM> liteInMemorySiteStorage,
        ICheckSiteService checkSiteService,
        IOptions<MonitorSitesConfiguration> configurations,
        ILogger<MonitorSitesStatusService> logger
       )
    {

        _liteInMemorySiteStorage = liteInMemorySiteStorage;
        _checkSiteService = checkSiteService;
        _inMemoryRegionStorage = inMemoryRegionStorage;
        _rateInMilliseconds = (int)TimeSpan.FromSeconds(configurations.Value.Rate).TotalMilliseconds;
        _logger = logger;
    }

    public async Task<int> MonitorAllAsync()
    {
        var timer = new Stopwatch();
        timer.Start();

        var allSites = _liteInMemorySiteStorage.GetAll();
        var allRegions = _inMemoryRegionStorage.GetAll();
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
                taskList.Add(CheckOneSiteOnAllRegionsAsync(item, allRegions));
            }

            await Task.Delay(_rateInMilliseconds / ITERATION_NUMBER);
        }

        await Task.WhenAll(taskList);

        timer.Stop();
        return timer.Elapsed.Seconds;
    }

    private async Task CheckOneSiteOnAllRegionsAsync(SiteVM site, IEnumerable<RegionVM> allRegions)
    {
        foreach (var region in allRegions)
        {
            await _checkSiteService.CheckAsync(site, region);
        }
    }
}
