using Dapper;
using Microsoft.EntityFrameworkCore;
using RussianSitesStatus.Database;
using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models.Dtos;

namespace RussianSitesStatus.Services;

public class DatabaseStorage
{
    // TODO: calculate dynamically based on the IRegionProvider.Regions.Count when it will be ready
    private const int NUMBER_OF_REGIONS = 3;

    private readonly ApplicationContext _db;

    public DatabaseStorage(ApplicationContext db)
    {
        _db = db;
    }

    public async Task<Site> GetSiteWithChecks(long siteId)
    {
        return await _db.Sites
            .Include(s => s.Checks)
            .Where(s => s.Id == siteId)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<Site> GetSite(long siteId)
    {
        return await _db.Sites.FindAsync(siteId);
    }

    public async Task<Site> GetSiteByUrl(string siteUrl)
    {
        return await _db.Sites
            .Where(s => s.Url == siteUrl)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<UpTimePerSiteDto>> GetAllUptime()
    {
        var connection = _db.Database.GetDbConnection();
        var upTime = await connection.QueryAsync<UpTimePerSiteDto>(
            @"select ""SiteId"", count(case when g.IsUp = true then g.""SiteId"" end)/count(""SiteId"")::float as UpTime from 
                   (select ""SiteId"", ""Iteration"", bool_or(sq.IsUp) as IsUp from
                        (select ""CheckedAt"", ""SiteId"", ""Iteration"", ""StatusCode"" >= 200 and ""StatusCode"" <= 300 as IsUp FROM public.""Checks""where ""CheckedAt"" >= (now() at time zone 'utc') - INTERVAL '24 HOURS') as sq
                    group by ""SiteId"", ""Iteration"") as g
                 group by ""SiteId""");
        return upTime;
    }

    public async Task<IEnumerable<Site>> GetAllSites()
    {
        return await _db.Sites
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<StatusPerSiteDto>> GetAllStatuses()
    {
        var connection = _db.Database.GetDbConnection();
        var statuses = await connection.QueryAsync<StatusPerSiteDto>(
            @"select ""SiteId"", min(sq.""Status"")
                 from (select ""CheckedAt"", ""SiteId"", ""Iteration"", ""Status""
                       from public.""Checks"" where ""Iteration"" = 
                                                    (select ""Iteration"" FROM public.""Checks"" order by ""CheckedAt"" limit 1)) as sq
                 group by ""SiteId"", ""Iteration""");
        return statuses;
    }

    public async Task<IEnumerable<Site>> GetAllSitesWithChecks()
    {
        return await _db.Sites
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Site>> SearchSitesByUrl(string url)
    {
        url = url.NormalizeSiteName();

        return await _db.Sites
            .AsNoTracking()
            .Where(s => s.Url.Contains(url))
            .ToListAsync();
    }

    public async Task<Site> AddSite(Site newSite)
    {
        _db.Sites.Add(newSite);

        await _db.SaveChangesAsync();

        return newSite;
    }

    public async Task AddSites(IEnumerable<Site> sites)
    {
        _db.Sites.AddRange(sites);

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Check>> GetLastCheckBySiteId(long siteId)
    {
        return await _db.Checks
            .Include(c => c.Region)
            .AsNoTracking()
            .Where(c => c.SiteId == siteId)
            .OrderByDescending(c => c.CheckedAt)
            .Take(NUMBER_OF_REGIONS)
            .ToListAsync();
    }

    public async Task<Check> AddCheck(Check newCheck)
    {
        _db.Checks.Add(newCheck);

        await _db.SaveChangesAsync();

        return newCheck;
    }

    public async Task AddChecksAsync(IEnumerable<Check> newChecks)
    {
        _db.Checks.AddRange(newChecks);

        await _db.SaveChangesAsync();
    }

    public async Task DeleteSite(long siteId)
    {
        var originalSite = _db.Sites.Find(siteId);
        _db.Sites.Remove(originalSite);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Region>> GetAllRegions()
    {
        return await _db.Regions
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Region> AddRegion(Region newRegion)
    {
        _db.Regions.Add(newRegion);

        await _db.SaveChangesAsync();

        return newRegion;
    }

    public async Task DeleteRegion(long regionId)
    {
        var originalRegion = _db.Regions.Find(regionId);
        _db.Regions.Remove(originalRegion);
        await _db.SaveChangesAsync();
    }
}