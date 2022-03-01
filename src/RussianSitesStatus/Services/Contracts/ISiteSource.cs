namespace RussianSitesStatus.Services.Contracts
{
    public interface ISiteSource
    {
        Task<IEnumerable<string>> GetAllAsync();
    }
}
