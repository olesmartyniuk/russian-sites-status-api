using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface ICheckSiteService
    {
        Task<Check> CheckAsync(SiteVM site, RegionVM region);
    }
}
