using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface IFetchDataService
    {
        Task<IEnumerable<SiteDetailsVM>> GetAllSitesDetailsAsync();
        Task<IEnumerable<RegionVM>> GetAllRegionsAsync();
    }
}
