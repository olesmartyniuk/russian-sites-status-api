using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface ICheckSiteService
    {
        Task<Check> CheckAsync(Site site, Region region, DateTime checkedAt);
    }
}
