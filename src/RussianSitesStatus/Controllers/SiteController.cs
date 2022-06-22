using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Auth;
using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;
using RussianSitesStatus.Services;
using RussianSitesStatus.Services.Contracts;
using System.Web;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteController : ControllerBase
{
    private readonly InMemoryStorage<Models.Site> _liteStatusStorage;
    private readonly InMemoryStorage<SiteDetails> _fullStatusStorage;
    private readonly DatabaseStorage _databaseStorage;
    private readonly StatisticStorage _statisticStorage;
    private readonly ICheckSiteService _checkSiteService;

    public SiteController(
        InMemoryStorage<Models.Site> liteStatusStorage,
        InMemoryStorage<SiteDetails> fullStatusStorage,
        DatabaseStorage databaseStorage,
        ICheckSiteService checkSiteService,
        StatisticStorage statisticStorage)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _databaseStorage = databaseStorage;
        _checkSiteService = checkSiteService;
        _statisticStorage = statisticStorage;
    }

    /// <summary>
    /// Returns all sites
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    GET https://api.mordor-sites-status.info/api/sites
    ///
    /// </remarks>    
    /// <returns>List of sites</returns>
    /// <response code="200">List of sites</response>
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites")]
    public ActionResult<List<Models.Site>> GetAll()
    {
        var result = _liteStatusStorage
            .GetAll()
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Returns site details by id
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    GET https://api.mordor-sites-status.info/api/sites/2876347
    ///
    /// </remarks>    
    /// <param name="id">Site id, integer number</param>
    /// <returns>Site details</returns>    
    /// <response code="200">Site details</response>
    /// <response code="404">Site not found</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/{id}")]
    public ActionResult<SiteDetails> Get(long id)
    {
        var result = _fullStatusStorage.Get(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Search for site by url
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    GET https://api.mordor-sites-status.info/api/sites/search?text=ya.ru
    ///
    /// </remarks>    
    /// <param name="text">Text, minimum 3 symbols</param>
    /// <returns>List of sites</returns>    
    /// <response code="200">List of sites</response>
    /// <response code="400">Bad request</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/search")]
    public ActionResult<IEnumerable<SiteDetails>> Search([FromQuery] string text)
    {
        if (string.IsNullOrEmpty(text) | text.Length < 3)
        {
            return BadRequest("The search text should contains more than 2 symbols");
        }

        var result = _fullStatusStorage.Search(text);
        return Ok(result);
    }

    /// <summary>
    /// Add site for monitoring
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    POST https://api.mordor-sites-status.info/api/sites/ya.ru
    ///
    /// </remarks>    
    /// <param name="siteUrl">Site URL to monitor</param>
    /// <response code="201">Created</response>
    /// <response code="400">Bad request</response> 
    /// <response code="401">Unauthorized</response> 
    /// <response code="409">Conflict</response> 
    /// <response code="500">Internal server error</response> 
    [HttpPost("api/sites/{siteUrl}")]
    [Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
    public async Task<ActionResult> Add([FromRoute()] string siteUrl)
    {
        siteUrl = HttpUtility
            .UrlDecode(siteUrl)
            .NormalizeSiteUrl();
        if (siteUrl.Length < 3)
        {
            return BadRequest("Site URL should be at least 3 symbols");
        }

        var originalSite = await _databaseStorage.GetSiteByUrl(siteUrl);
        if (originalSite != null)
        {
            return Conflict(originalSite);
        }

        var site = new Database.Models.Site
        {
            Name = siteUrl.NormalizeSiteName(),
            CreatedAt = DateTime.UtcNow,
            Url = siteUrl
        };

        var newSite = await _databaseStorage.AddSite(site);

        return CreatedAtAction(nameof(Get), new { id = newSite.Id }, newSite);
    }

    /// <summary>
    /// Remove site from monitoring
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    DELETE https://api.mordor-sites-status.info/api/sites/82197
    ///
    /// </remarks>    
    /// <param name="siteId">Site Id to remove</param>
    /// <response code="204">No content</response>
    /// <response code="401">Unauthorized</response> 
    /// <response code="404">Not found</response> 
    /// <response code="500">Internal server error</response> 
    [HttpDelete("api/sites/{siteId}")]
    [Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
    public async Task<ActionResult> Delete(long siteId)
    {
        var originalSite = await _databaseStorage.GetSite(siteId);
        if (originalSite == null)
        {
            return NotFound($"Site is not found by id '{siteId}'.");
        }

        await _databaseStorage.DeleteSite(originalSite.Id);

        return NoContent();
    }

    /// <summary>
    /// Check site status by url. This call may take some time. Up to 30 sec.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    GET https://api.mordor-sites-status.info/api/sites/search?text=ya.ru
    ///
    /// </remarks>    
    /// <param name="siteUrl">Site url with protocol</param>
    /// <returns>Site status details</returns>    
    /// <response code="200">Site status details</response>
    /// <response code="400">Bad request</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/check/{siteUrl}")]
    public async Task<ActionResult<SiteDetails>> Check(string siteUrl)
    {
        if (string.IsNullOrEmpty(siteUrl) | siteUrl.Length < 3)
        {
            return BadRequest("The search text should contains more than 2 symbols");
        }

        var regions = await _databaseStorage.GetRegions(true);
        var site = await _checkSiteService.CheckByUrl(siteUrl, regions);

        var result = new SiteDetails
        {
            Id = site.Id,
            Name = site.Name,
            Status = GetSiteStatus(site.Checks.Max(c => c.Status)),
            Uptime = 0,
            Servers = GetServers(site.Checks),
            LastTestedAt = site.CheckedAt
        };

        return Ok(result);
    }

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

    private List<Server> GetServers(ICollection<Check> checks)
    {
        var servers = new List<Server>();

        foreach (var check in checks)
            servers.Add(new Server
            {
                Region = check.Region.Name,
                RegionCode = check.Region.Code,
                Status = GetSiteStatus(check.Status),
                StatusCode = check.StatusCode,
                SpentTimeInSec = check.SpentTime
            });

        return servers;
    }

    private string GetSiteStatus(CheckStatus checkStatus)
    {
        var result = checkStatus switch
        {
            CheckStatus.Available => SiteStatus.Up,
            CheckStatus.Unavailable => SiteStatus.Down,
            CheckStatus.Unknown => SiteStatus.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(checkStatus), $"Not expected site status value: {checkStatus}"),
        };
        return result;
    }
}
