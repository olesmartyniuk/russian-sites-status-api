using RussianSitesStatus.Database.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface ICheckSiteService
    {
        Task CheckAsync(Site site, Proxy region);
    }
}
