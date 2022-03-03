using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Extensions;
using RussianSitesStatus.Services.Contracts;
using System.Collections.Concurrent;

namespace RussianSitesStatus.Services.StatusCake;

public class SyncSitesDatabaseService : ISyncSitesService
{
    private readonly IEnumerable<ISiteSource> _siteSources;
    private readonly DatabaseStorage _database;
    private readonly ILogger<SyncSitesStatusCakeService> _logger;

    public SyncSitesDatabaseService(
        IEnumerable<ISiteSource> siteSources,
        DatabaseStorage database,
        ILogger<SyncSitesStatusCakeService> logger)
    {
        _siteSources = siteSources;
        _database = database;

        _logger = logger;
    }

    public async Task SyncAsync()
    {
        var allSitesFromSources = await GetSitesFromAllSources();
        var siteUrlsToAdd = await GetSiteUrlsToAdd(allSitesFromSources);

        await AddNewSites(siteUrlsToAdd);
    }

    private async Task AddNewSites(IEnumerable<string> siteUrls)
    {
        var sites = siteUrls
            .Select(siteUrl => new Site
            {
                Url = siteUrl,
                Name = siteUrl.NormalizeSiteName(),
                CreatedAt = DateTime.UtcNow
            });

        await _database.AddSites(sites);
    }

    private async Task<IEnumerable<string>> GetSitesFromAllSources()
    {
        var allSites = new ConcurrentBag<string>();
        var taskList = new List<Task>();
        foreach (var siteSource in _siteSources)
        {
            var action = async () =>
            {
                try
                {
                    var sites = (await siteSource.GetAllAsync())
                        .Where(url => url.IsValid())
                        .Select(url => url.NormalizeSiteUrl());

                    allSites.Add(sites);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $@"Unhandled exception while siteUrls from {siteSource.GetType().Name}");
                }
            };

            taskList.Add(Task.Run(action));
        }

        await Task.WhenAll(taskList);

        return allSites.Distinct().ToList();
    }

    private async Task<IEnumerable<string>> GetSiteUrlsToAdd(IEnumerable<string> allSiteUrls)
    {
        var existingSites = await _database.GetAllSites();
        var existingSiteUrls = existingSites.Select(s => s.Url);

        return allSiteUrls
            .Except(existingSiteUrls);
    }
}
