using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.NameTranslation;
using NpgsqlTypes;
using RussianSitesStatus.Database;
using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models.Dtos;
using System.Data;
using System.Data.SqlClient;

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
            @"select
	            site_id as SiteId,
	            count(case when g.IsUp = true then g.site_id end)/ count(site_id)::float as UpTime
            from
	            (
	            select
		            site_id, checked_at, bool_or(sq.IsUp) as IsUp
	            from
		            (
		            select
			            checked_at, site_id, status_code >= 200
			            and status_code <= 300 as IsUp
		            from
			            checks c where checked_at >= (now() at time zone 'utc') - interval '24 HOURS') as sq
	            group by
		            site_id, checked_at) as g
            group by
	            site_id");
        return upTime;
    }

    public async Task<IEnumerable<Site>> GetAllSites()
    {
        return await _db.Sites
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Site>> GetAllSitesWithLastChecks()
    {
        var sites = await _db.Sites
           .AsNoTracking()
           .ToListAsync();

        var regions = await _db.Regions
            .ToListAsync();
        var regionsById = regions
            .ToDictionary(r => r.Id, r => r);

        var connection = _db.Database.GetDbConnection();
        var checks = await connection.QueryAsync<Check>(
            @"select
                ch.id as Id,
	            ch.site_id as SiteId,
                ch.status as Status,
                ch.status_code as StatusCode,
                ch.spent_time as SpentTime,
                ch.checked_at as CheckedAt,
                ch.region_id as RegionId
            from
                checks as ch
            join 
	            sites as st on st.id = ch.site_id 
            where
                st.checked_at = ch.checked_at 
            order by
	            ch.site_id");

        var checksBySiteId = checks
            .GroupBy(c => c.SiteId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var site in sites)
        {
            if (checksBySiteId.ContainsKey(site.Id))
            {
                var lastChecks = checksBySiteId[site.Id];
                foreach (var check in lastChecks)
                {
                    if (regionsById.ContainsKey(check.RegionId))
                    {
                        check.Region = regionsById[check.RegionId];
                    }

                    site.Checks.Add(check);
                }
            }
        }

        return sites;
    }

    public async Task<IEnumerable<StatusPerSiteDto>> GetAllStatuses()
    {
        var connection = _db.Database.GetDbConnection();
        var statuses = await connection.QueryAsync<StatusPerSiteDto>(
            @"select
	            site_id as SiteId,
	            min(sq.status) as Status
            from
	            (
	            select
		            checked_at, site_id, status
	            from
		            checks
	            where
		            checked_at = (
		            select
			            max(checked_at)
		            from
			            checks)) as sq
            group by
	            site_id,
	            checked_at");
        return statuses;
    }

    public async Task UpdateCheckedAt(DateTime checkedAt)
    {
        var commandText =
            @"update 
                sites 
              set 
                checked_at = @CheckedAt";

        var checkedAtParam = new NpgsqlParameter("@CheckedAt", checkedAt);
        await _db.Database.ExecuteSqlRawAsync(commandText, checkedAtParam);
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

    public async Task<IEnumerable<Region>> GetRegions(bool onlyActive = false)
    {
        var query = _db.Regions
            .AsNoTracking();

        if (onlyActive)
        {
            query = query.Where(r => r.ProxyIsActive);
        }

        return await query.ToListAsync();
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

    public async Task<DateTime> GetOldestCheckSiteDate()
    {
        var commandText = @"SELECT MIN(checked_at) FROM checks;";

        var connection = _db.Database.GetDbConnection();
        return await connection.QuerySingleAsync<DateTime>(commandText);
    }

    public async Task<IEnumerable<int>> GetUniqueSiteIds()
    {
        var commandText = @"SELECT DISTINCT site_id FROM checks;";

        var connection = _db.Database.GetDbConnection();
        return await connection.QueryAsync<int>(commandText);
    }

    public async Task<IEnumerable<StatisticInfo>> CalculateStatisticAsync(int siteId, DateTime date)
    {
        var commandText = @"SELECT * FROM fn_calculate_statistic_per_day(@siteId, @date)";

        var parameters = new DynamicParameters();
        parameters.Add("@siteId", siteId);
        parameters.Add("@date", date, DbType.Date);

        var connection = _db.Database.GetDbConnection();
        var result = await connection.QueryAsync<StatisticInfo>(commandText, parameters);
        return result;
    }

    public async Task AddChecksStatisticsAsync(ChecksStatistics checksStatistics)
    {
        _db.ChecksStatistics.Add(checksStatistics);

        await _db.SaveChangesAsync();
    }

    public async Task DeleteStatistis(DateTime endDate)
    {
        var commandText = @"DELETE FROM checks WHERE checked_at <= @end_date;";

        var endDateParam = new NpgsqlParameter("@end_date", endDate);
        await _db.Database.ExecuteSqlRawAsync(commandText, endDateParam);
    }

    public async Task<DateTime> GetNewestStatistisDate()
    {
        var commandText = @"SELECT MAX(day) FROM ChecksStatistics;";

        var connection = _db.Database.GetDbConnection();
        return await connection.QuerySingleAsync<DateTime>(commandText);
    }
}