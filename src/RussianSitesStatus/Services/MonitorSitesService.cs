using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Services.Contracts;
using RussianSitesStatus.Services.StatusCake;
using Region = RussianSitesStatus.Database.Models.Region;

namespace RussianSitesStatus.Services;
public class MonitorSitesService
{
    private static int Iterations = 10;
    private static int IterationDuration = 60000;

    private readonly BaseStorage<Site> _inMemorySiteStorage;
    private readonly BaseStorage<Region> _inMemoryRegionStorage;
    private readonly ICheckSiteService _checkSiteService;

    private readonly ILogger<SyncSitesService> _logger;

    public MonitorSitesService(
        IConfiguration configuration,
        BaseStorage<Site> liteStatusStorage,
        ILogger<SyncSitesService> logger,
        StatusCakeUpCheckService upCheckService,
        BaseStorage<Region> inMemoryRegionStorage,
        ICheckSiteService checkSiteService)
    {

        _logger = logger;
        _inMemoryRegionStorage = inMemoryRegionStorage;
        _inMemorySiteStorage = liteStatusStorage;
        _checkSiteService = checkSiteService;
    }

    public async Task MonitorAllAsync()
    {
        var allSites = _inMemorySiteStorage.GetAll();
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
    }

    private async Task CheckOneSiteOnAllRegionsAsync(Site site)
    {
        var allRegions = _inMemoryRegionStorage.GetAll();
        foreach (var region in allRegions)
        {
            await _checkSiteService.CheckAsync(site, region);
        }
    }
}
