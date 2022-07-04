using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly DatabaseStorage _databaseStorage;
    private readonly StatisticStorage _statisticStorage;

    public StatisticsController(
        DatabaseStorage databaseStorage,
        StatisticStorage statisticStorage)
    {
        _databaseStorage = databaseStorage;
        _statisticStorage = statisticStorage;
    }
    
    /// <summary>
    /// Get statistics data for default period (last day).
    /// </summary>
    /// <param name="siteId">Site id</param>
    /// <returns>Statistics details</returns>    
    /// <response code="200">Statistics details</response>
    /// <response code="400">Bad request</response> 
    /// <response code="404">Sie not found by id</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/{siteId}/statistics")]
    public async Task<ActionResult<Models.Statistic>> GetStatisticDefault(long siteId)
    {
        var site = await _databaseStorage.GetSite(siteId);
        if (site == null)
        {
            return NotFound($"Site with id '{siteId}' was not found.");
        }

        var now = DateTime.UtcNow;
        var periodStart = new DateTime(now.Year, now.Month, now.Day);
        var periodEnd = periodStart.AddDays(1);
        var data = _statisticStorage.GetData(site, periodStart, periodEnd);

        return Ok(StatisticDtoBuilder.GetForDay(data, periodStart, site));
    }

    /// <summary>
    /// Get statistics data for specific day.
    /// </summary>
    /// <param name="siteId">Site id</param>
    /// <param name="year">Year</param>
    /// <param name="month">Month</param>
    /// <param name="day">Day</param>
    /// <returns>Statistics details</returns>    
    /// <response code="200">Statistics details</response>
    /// <response code="400">Bad request</response> 
    /// <response code="404">Sie not found by id</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/{siteId}/statistics/period/day/date/{year}/{month}/{day}")]
    public async Task<ActionResult<Models.Statistic>> GetStatisticByDay(long siteId, int year, int month, int day)
    {
        var site = await _databaseStorage.GetSite(siteId);
        if (site == null)
        {
            return NotFound($"Site with id '{siteId}' was not found.");
        }

        var periodStart = new DateTime(year, month, day);
        var periodEnd = periodStart.AddDays(1);
        var data = _statisticStorage.GetData(site, periodStart, periodEnd);
        
        return Ok(StatisticDtoBuilder.GetForDay(data, periodStart, site));
    }

    /// <summary>
    /// Get statistics data for specific week.
    /// </summary>
    /// <param name="siteId">Site id</param>
    /// <param name="year">Year</param>
    /// <param name="week">Week</param>    
    /// <returns>Statistics details</returns>    
    /// <response code="200">Statistics details</response>
    /// <response code="400">Bad request</response> 
    /// <response code="404">Sie not found by id</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/{siteId}/statistics/period/week/date/{year}/{week}")]
    public async Task<ActionResult<Models.Statistic>> GetStatisticByWeek(long siteId, int year, int week)
    {
        var site = await _databaseStorage.GetSite(siteId);
        if (site == null)
        {
            return NotFound($"Site with id '{siteId}' was not found.");
        }

        var periodStart = GetWeekStartDate(year, week);
        var periodEnd = periodStart.AddDays(7);
        var data = _statisticStorage.GetData(site, periodStart, periodEnd);

        return Ok(StatisticDtoBuilder.GetForWeek(data, periodStart, site));
    }

    /// <summary>
    /// Get statistics data for specific month.
    /// </summary>
    /// <param name="siteId">Site id</param>
    /// <param name="year">Year</param>
    /// <param name="month">Month</param>    
    /// <returns>Statistics details</returns>    
    /// <response code="200">Statistics details</response>
    /// <response code="400">Bad request</response> 
    /// <response code="404">Sie not found by id</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/{siteId}/statistics/period/month/date/{year}/{month}")]
    public async Task<ActionResult<Models.Statistic>> GetStatisticByMonth(long siteId, int year, int month)
    {
        var site = await _databaseStorage.GetSite(siteId);
        if (site == null)
        {
            return NotFound($"Site with id '{siteId}' was not found.");
        }

        var periodStart = new DateTime(year, month, 1);        
        var periodEnd = periodStart.AddMonths(1);
        var data = _statisticStorage.GetData(site, periodStart, periodEnd);             

        return Ok(StatisticDtoBuilder.GetForMonth(data, periodStart, site));
    }

    private DateTime GetWeekStartDate(int year, int week)
    {
        DateTime jan1 = new DateTime(year, 1, 1);
        int day = (int)jan1.DayOfWeek - 1;
        int delta = (day < 4 ? -day : 7 - day) + 7 * (week - 1);

        return jan1.AddDays(delta);
    }
}
