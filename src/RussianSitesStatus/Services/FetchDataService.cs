using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Dtos;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.Services
{
    public class FetchDataService : IFetchDataService
    {
        private IServiceScopeFactory _serviceScopeFactory;
        public FetchDataService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }


        public async Task<IEnumerable<SiteDetailsVM>> GetAllSitesDetailsAsync()
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

                var siteDetailsVMList = new List<SiteDetailsVM>();
                var sitesDB = await databaseStorage.GetAllSites();
                var statuses = (await databaseStorage.GetAllStatuses()).ToDictionary(x => x.SiteId, y => y.Status);
                var uptime = (await databaseStorage.GetAllUptime()).ToDictionary(x => x.SiteId, y => y.UpTime);

                foreach (var siteDbItem in sitesDB)
                {
                    siteDetailsVMList.Add(GetSiteDetailsVM(siteDbItem, uptime, statuses));
                }

                return siteDetailsVMList;
            }
        }

        private string GetSiteStatus(long siteId, IReadOnlyDictionary<long, SiteStatus> statusPerSite)
        {
            if (statusPerSite.TryGetValue(siteId, out var siteStatus))
            {
                var result = siteStatus switch
                {
                    SiteStatus.Down => "Down",
                    SiteStatus.Up => "Up",
                    SiteStatus.FailToIdentify => "Unknown",
                    _ => throw new ArgumentOutOfRangeException(nameof(siteStatus), $"Not expected site status value: {siteStatus}"),
                };
                return result;
            }
            return "Unknown";
        }

        private SiteDetailsVM GetSiteDetailsVM(Site siteDbItem, IReadOnlyDictionary<long, float> uptimePerSite, IReadOnlyDictionary<long, SiteStatus> statusPerSite)
        {
            var lastItem = siteDbItem.Checks.OrderBy(check => check.CheckedAt).LastOrDefault();

            var status = GetSiteStatus(siteDbItem.Id, statusPerSite);
            var uptime = uptimePerSite.TryGetValue(siteDbItem.Id, out var result1) ? (float?)result1 * 100 : null;
            return new SiteDetailsVM
            {
                Id = siteDbItem.Id.ToString(),
                Name = siteDbItem.Name,
                TestType = "HTTP",
                WebsiteUrl = siteDbItem.Url,
                Status = status,
                Uptime = uptime,
                Servers = GetServers(siteDbItem.Checks),
                Timeout = lastItem?.SpentTime ?? 0,
                LastTestedAt = lastItem?.CheckedAt ?? default(DateTime)
            };
        }

        private List<ServerDto> GetServers(ICollection<Check> Checks)
        {
            var servers = new List<ServerDto>();

            var lastCheck = Checks.OrderBy(check => check.CheckedAt).LastOrDefault();
            foreach (var check in Checks)
                servers.Add(new ServerDto
                {
                    Region = check.Region.Name,
                    RegionCode = check.Region.Name,
                    Status = lastCheck.Status.ToString(),
                    StatusCode = lastCheck.StatusCode,
                    LastTestedAt = lastCheck.CheckedAt,
                });

            return servers;
        }

        public async Task<IEnumerable<RegionVM>> GetAllRegionsAsync()
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

                var regions = await databaseStorage.GetAllRegions();
                return regions.Select(region => new RegionVM
                {
                    Id = region.Id,
                    Name = region.Name,
                    Code = region.Code,
                    ProxyUrl = region.ProxyUrl,
                    ProxyUser = region.ProxyUser,
                    ProxyPassword = region.ProxyPassword,
                    ProxyIsActive = region.ProxyIsActive
                }); //TODOVK: use automapper
            }
        }
    }
}
