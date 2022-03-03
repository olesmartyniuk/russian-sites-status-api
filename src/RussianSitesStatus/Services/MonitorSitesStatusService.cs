using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;
using System.Diagnostics;

namespace RussianSitesStatus.Services;
public class MonitorSitesStatusService
{
    private static int Iterations = 10;
    private static int IterationDuration = 60000;

    private readonly InMemoryStorage<SiteVM> _liteInMemorySiteStorage;
    private readonly BaseInMemoryStorage<Region> _inMemoryRegionStorage;
    private readonly ICheckSiteService _checkSiteService;
    private readonly ILogger<MonitorSitesStatusService> _logger;

    public MonitorSitesStatusService(
         BaseInMemoryStorage<Region> inMemoryRegionStorage,
        InMemoryStorage<SiteVM> liteInMemorySiteStorage,
        ICheckSiteService checkSiteService,
        ILogger<MonitorSitesStatusService> logger
       )
    {

        _logger = logger;
        _liteInMemorySiteStorage = liteInMemorySiteStorage;
        _checkSiteService = checkSiteService;
        _inMemoryRegionStorage = inMemoryRegionStorage;
    }

    public async Task<int> MonitorAllAsync()
    {
        var timer = new Stopwatch();
        timer.Start();

        var allSites = _liteInMemorySiteStorage.GetAll();
        var taskList = new List<Task>();
        foreach (var batch in allSites.Chunk(allSites.Count() / Iterations))
        {
            foreach (var item in batch)
            {
                taskList.Add(CheckOneSiteOnAllRegionsAsync(item));
            }

            await Task.Delay(IterationDuration / Iterations);
        }

        await Task.WhenAll(taskList);

        timer.Stop();
        return timer.Elapsed.Seconds;
    }

    private async Task CheckOneSiteOnAllRegionsAsync(SiteVM site)
    {
        var allRegions = _inMemoryRegionStorage.GetAll();
        foreach (var region in allRegions)
        {
            await _checkSiteService.CheckAsync(site, region);
        }
    }
}
