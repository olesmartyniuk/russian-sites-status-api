using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;
using System.Diagnostics;

namespace RussianSitesStatus.Services;
public class CheckSiteService : ICheckSiteService
{
    private readonly DatabaseStorage _databaseStorage;
    private readonly HttpClient _httpClient;

    public CheckSiteService(IServiceScopeFactory serviceScopeFactory)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        using (var serviceScope = serviceScopeFactory.CreateScope())
        {
            _databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();
        }

    }

    // make api call
    //save Ceck: status code, SpentTime
    public async Task CheckAsync(SiteVM site, Region region)
    {
        try
        {
            Stopwatch timer = new Stopwatch();

            timer.Start();

            //TODOKhrystyna: Make api call to proxy server
            var response = await _httpClient.GetAsync(site.WebsiteUrl);

            timer.Stop();

            await SaveCheckAsync((int)response.StatusCode, site, region, timer.Elapsed.Seconds);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task SaveCheckAsync(int statusCode, SiteVM site, Region region, int spentTime)
    {
        var check = new Check
        {
            CheckedAt = DateTime.UtcNow,
            SiteId = int.Parse(site.Id),
            StatusCode = statusCode,
            SpentTime = spentTime,
            RegionId = region.Id
        };

        await _databaseStorage.AddCheck(check);
    }
}
