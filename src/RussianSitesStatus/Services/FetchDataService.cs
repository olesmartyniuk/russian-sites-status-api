using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;
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

                var sitesDB = await databaseStorage
                    .GetAllSitesWithLastChecks();

                var statuses = (await databaseStorage.GetAllStatuses())
                    .ToDictionary(x => x.SiteId, y => y.Status);

                var uptime = (await databaseStorage.GetAllUptime())
                    .ToDictionary(x => x.SiteId, y => y.UpTime);

                foreach (var siteDbItem in sitesDB)
                {
                    siteDetailsVMList.Add(GetSiteDetailsVM(siteDbItem, uptime, statuses));
                }

                return siteDetailsVMList;
            }
        }

        public string GetSiteStatus(CheckStatus checkStatus)
        {
            var result = checkStatus switch
            {
                CheckStatus.Available => SiteStatus.Up,
                CheckStatus.Unavailable => SiteStatus.Down,
                CheckStatus.Unknown => SiteStatus.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(checkStatus), $"Not expected site status value: {checkStatus}"),
            };
            return result;
        }

        private SiteDetailsVM GetSiteDetailsVM(
            Site siteDbItem, 
            IReadOnlyDictionary<long, float> uptimePerSite, 
            IReadOnlyDictionary<long, CheckStatus> statusPerSite)
        {
            var lastItem = siteDbItem
                .Checks
                .OrderBy(check => check.CheckedAt).LastOrDefault();

            var status =  statusPerSite.TryGetValue(siteDbItem.Id, out var siteStatus) 
                ? GetSiteStatus(siteStatus) 
                : SiteStatus.Unknown;
            
            var uptime = uptimePerSite.TryGetValue(siteDbItem.Id, out var result) 
                ? (float?)result * 100 
                : null;
            
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

        private List<ServerDto> GetServers(ICollection<Check> checks)
        {
            var servers = new List<ServerDto>();
            
            var lastCheck = checks
                .OrderBy(check => check.CheckedAt)
                .LastOrDefault();
            
            foreach (var check in checks)
                servers.Add(new ServerDto
                {
                    Region = check.Region.Name,
                    RegionCode = check.Region.Code,
                    Status = GetSiteStatus(lastCheck!.Status),
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

                var regions = await databaseStorage.GetRegions();
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
