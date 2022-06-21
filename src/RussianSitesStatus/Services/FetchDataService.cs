using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;
using RussianSitesStatus.Services.Contracts;
using Site = RussianSitesStatus.Database.Models.Site;

namespace RussianSitesStatus.Services
{
    public class FetchDataService : IFetchDataService
    {
        private IServiceScopeFactory _serviceScopeFactory;
        public FetchDataService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }


        public async Task<IEnumerable<SiteDetails>> GetAllSitesDetailsAsync()
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

                var siteDetailsVMList = new List<SiteDetails>();

                var sitesDB = await databaseStorage
                    .GetAllSitesWithLastChecks();

                var statuses = await databaseStorage.GetAllStatuses();

                var statusesBySiteId = statuses
                    .ToDictionary(x => x.SiteId, y => y.Status);

                var uptime = (await databaseStorage.GetAllUptime())
                    .ToDictionary(x => x.SiteId, y => y.UpTime);

                foreach (var siteDbItem in sitesDB)
                {
                    siteDetailsVMList.Add(GetSiteDetailsVM(siteDbItem, uptime, statusesBySiteId));
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

        private SiteDetails GetSiteDetailsVM(
            Site siteDbItem, 
            IReadOnlyDictionary<long, float> uptimePerSite, 
            IReadOnlyDictionary<long, CheckStatus> statusPerSite)
        {
            var status =  statusPerSite.TryGetValue(siteDbItem.Id, out var siteStatus) 
                ? GetSiteStatus(siteStatus) 
                : SiteStatus.Unknown;
            
            var uptime = uptimePerSite.TryGetValue(siteDbItem.Id, out var result) 
                ? (float?)result * 100 
                : null;
            
            return new SiteDetails
            {
                Id = siteDbItem.Id,
                Name = siteDbItem.Name,                
                Status = status,
                Uptime = uptime,               
                Servers = GetServers(siteDbItem.Checks),                
                LastTestedAt = siteDbItem.CheckedAt
            };
        }

        private List<Server> GetServers(ICollection<Check> checks)
        {
            var servers = new List<Server>();
            
            foreach (var check in checks)
                servers.Add(new Server
                {
                    Region = check.Region.Name,
                    RegionCode = check.Region.Code,
                    Status = GetSiteStatus(check.Status),
                    StatusCode = check.StatusCode,
                    SpentTimeInSec = check.SpentTime
                });

            return servers;
        }

        public async Task<IEnumerable<Models.Region>> GetAllRegionsAsync()
        {
            using (var serviceScope = _serviceScopeFactory.CreateScope())
            {
                var databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

                var regions = await databaseStorage.GetRegions();
                return regions.Select(region => new Models.Region
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
