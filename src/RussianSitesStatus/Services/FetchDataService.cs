using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
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

                foreach (var siteDbItem in sitesDB)
                {
                    siteDetailsVMList.Add(GetSiteDetailsVM(siteDbItem));
                }

                return siteDetailsVMList;
            }
        }

        private SiteDetailsVM GetSiteDetailsVM(Site siteDbItem)
        {
            var lastItem = siteDbItem.Checks.OrderBy(check => check.CheckedAt).LastOrDefault();

            var latsChecks = siteDbItem.Checks.Where(check => check.CheckedAt > DateTime.UtcNow.AddDays(-1)).ToList();
            var countChecks = latsChecks.Count();
            var countSuccess = latsChecks.Where(check => check.Status == Database.Models.CheckStatus.Available).Count();

            return new SiteDetailsVM
            {
                Id = siteDbItem.Id.ToString(),
                Name = siteDbItem.Name,
                TestType = "HTTP",
                WebsiteUrl = siteDbItem.Url,
                Status = lastItem?.Status.ToString(),
                Uptime = countChecks > 0 ? countSuccess * 100 / countChecks : 100,

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
                return regions.Select(region => new RegionVM { Id = region.Id, Name = region.Name, ProxyUrl = region.ProxyUrl, ProxyUser = region.ProxyUser, ProxyPassword = region.ProxyPassword }); //TODOVK: use automapper
            }
        }
    }
}
