using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface IFetchDataService
    {
        Task<IEnumerable<SiteVM>> GetAllAsync();

        Task<IEnumerable<SiteDetailsVM>> GetAllSitesDetailsAsync(IEnumerable<SiteVM> liteModels = null);
    }
}
