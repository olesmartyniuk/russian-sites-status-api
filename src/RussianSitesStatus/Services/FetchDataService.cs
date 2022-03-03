using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.Services
{
    public class FetchDataService : IFetchDataService
    {
        private DatabaseStorage _databaseStorage;
        public FetchDataService(IServiceScopeFactory serviceScopeFactory)
        {
            using (var serviceScope = serviceScopeFactory.CreateScope())
            {
                _databaseStorage = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();
            }
        }

        public async Task<IEnumerable<SiteDetailsVM>> GetAllSitesDetailsAsync()
        {
            var siteDetailsVMList = new List<SiteDetailsVM>();
            var sitesDB = await _databaseStorage.GetAllSites();

            foreach (var siteDbItem in sitesDB)
            {
                siteDetailsVMList.Add(GetSiteDetailsVM(siteDbItem));
            }

            return siteDetailsVMList;
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
                Status = lastItem.Status.ToString(),
                Uptime = countChecks > 0 ? countSuccess * 100 / countChecks : 100,

                Servers = GetServers(siteDbItem.Checks),
                Timeout = lastItem.SpentTime,
                LastTestedAt = lastItem.CheckedAt
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
            var regions = await _databaseStorage.GetAllRegions();
            return regions.Select(region => new RegionVM { Id = region.Id, Name = region.Name, ProxyUrl = region.ProxyUrl }); //TODOVK: use automapper
        }
    }
}
