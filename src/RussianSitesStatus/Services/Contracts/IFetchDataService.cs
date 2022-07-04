using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface IFetchDataService
    {
        Task<IEnumerable<SiteDetails>> GetAllSitesDetailsAsync();
        Task<IEnumerable<Region>> GetAllRegionsAsync();
    }
}
