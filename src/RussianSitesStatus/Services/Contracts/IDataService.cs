using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface IDataService
    {
        Task<IEnumerable<SiteVM>> GetAllAsync();

        Task<IEnumerable<SiteDetailsVM>> GetAllSitesDetailsAsync(IEnumerable<SiteVM> liteModels = null);
    }
}
