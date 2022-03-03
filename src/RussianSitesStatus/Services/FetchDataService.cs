using RussianSitesStatus.Models;
using RussianSitesStatus.Services.Contracts;

namespace RussianSitesStatus.Services
{
    public class FetchDataService : IFetchDataService
    {
        private DatabaseStorage _databaseStorage;
        public FetchDataService(DatabaseStorage databaseStorage)
        {
            _databaseStorage = databaseStorage;
        }

        public async Task<IEnumerable<SiteVM>> GetAllAsync()
        {
            var siteVMList = new List<SiteVM>();
            var sitesDB = await _databaseStorage.GetAllSites();

            foreach (var siteDbItem in sitesDB)
            {
                var lastStatus = siteDbItem.Checks.OrderBy(check => check.CheckedAt).LastOrDefault().Status;

                var latsChecks = siteDbItem.Checks.Where(check => check.CheckedAt > DateTime.UtcNow.AddDays(-1)).ToList();
                var countChecks = latsChecks.Count();
                var countSuccess = latsChecks.Where(check => check.Status == Database.Models.CheckStatus.Available) .Count();

                siteVMList.Add(new SiteVM
                {
                    Id = siteDbItem.Id.ToString(),
                    Name = siteDbItem.Name,
                    TestType = "HTTP",
                    WebsiteUrl = siteDbItem.Url,
                    Status = lastStatus.ToString(),
                    Uptime = countChecks > 0 ? countSuccess * 100 / countChecks : 100,
                });
            }

            return siteVMList;
        }

        public Task<IEnumerable<SiteDetailsVM>> GetAllSitesDetailsAsync(IEnumerable<SiteVM> liteModels = null)
        {
            throw new NotImplementedException();
        }
    }
}
