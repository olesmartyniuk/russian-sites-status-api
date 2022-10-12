using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Auth;
using RussianSitesStatus.Extensions;
using RussianSitesStatus.Services;
using System.Web;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class AdministrationController : ControllerBase
{
    private readonly DatabaseStorage _databaseStorage;
    private readonly CleanupChecksService _cleanupChecksService;
    private readonly ArchiveStatisticService _archiveStatisticService;

    public AdministrationController(
        DatabaseStorage databaseStorage,
        CleanupChecksService cleanupChecksService,
        ArchiveStatisticService archiveStatisticService)
    {
        _databaseStorage = databaseStorage;
        _cleanupChecksService = cleanupChecksService;
        _archiveStatisticService = archiveStatisticService;
    }

    /// <summary>
    /// ADMINS ONLY. Cleanup checks that were already stored in the statistics.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    POST https://api.mordor-sites-status.info/api/checks/cleanup
    ///
    /// </remarks>                
    /// <response code="401">Unauthorized</response>     
    /// <response code="500">Internal server error</response> 
    [HttpPost("api/checks/cleanup")]
   // [Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
    public async Task<ActionResult> Cleanup()
    {
        var result = await _cleanupChecksService.CleanupOldData();

        return Ok(new { success = result, message = "Check logs for details." });
    }

    /// <summary>
    /// ADMINS ONLY. Archive old checks and make statistic data.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    POST https://api.mordor-sites-status.info/api/checks/archive
    ///
    /// </remarks>                
    /// <response code="401">Unauthorized</response>     
    /// <response code="500">Internal server error</response> 
    [HttpPost("api/checks/archive")]
   // [Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
    public async Task<ActionResult> Archive()
    {
        var result = await _archiveStatisticService.ArchiveStatistic();

        return Ok(new { success = result, message = "Check logs for details." });
    }

    /// <summary>
    /// ADMINS ONLY. Add site for monitoring
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

        return CreatedAtAction("Get", new { id = newSite.Id }, newSite);
    }

    /// <summary>
    /// ADMINS ONLY. Remove site from monitoring
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
}
