using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Services.Contracts;
using System.Diagnostics;

namespace RussianSitesStatus.Services;
public class CheckSiteService : ICheckSiteService
{
    private readonly DatabaseStorage _databaseStorage;
    private readonly HttpClient _httpClient;

    public CheckSiteService(DatabaseStorage databaseStorage, HttpClient httpClient)
    {
        _databaseStorage = databaseStorage;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    // make api call
    //save Ceck: status code, SpentTime
    public async Task CheckAsync(Site site, Proxy region)
    {
        try
        {
            Stopwatch timer = new Stopwatch();

            timer.Start();

            //TODOKhrystyna: Make api call to proxy server
            var response = await _httpClient.GetAsync(site.Url);

            timer.Stop();

            await SaveCheckAsync((int)response.StatusCode, site, region, timer.Elapsed.Seconds);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task SaveCheckAsync(int statusCode, Site site, Proxy region, int spentTime)
    {
        var check = new Check
        {
            CheckedAt = DateTime.UtcNow,
            Site = site,
            StatusCode = statusCode,
            SpentTime = spentTime,
            Region = region
        };

        await _databaseStorage.AddCheck(check);
    }
}
