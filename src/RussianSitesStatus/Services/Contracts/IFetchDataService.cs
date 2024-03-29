﻿using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services.Contracts
{
    public interface IFetchDataService
    {
        Task<IEnumerable<SiteDetails>> GetAllSitesDetails();
        Task<IEnumerable<Region>> GetAllRegions();
    }
}
