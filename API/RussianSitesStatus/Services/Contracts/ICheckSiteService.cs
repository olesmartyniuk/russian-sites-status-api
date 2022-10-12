using RussianSitesStatus.Database.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface ICheckSiteService
    {
        Task<Check> Check(Site site, Region region, DateTime checkedAt);

        Task<Site> CheckByUrl(string siteUrl, IEnumerable<Region> region);
    }
}
