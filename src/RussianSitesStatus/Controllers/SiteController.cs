using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Extensions;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;
using System.Web;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteController : ControllerBase
{
    private readonly InMemoryStorage<SiteVM> _liteStatusStorage;
    private readonly InMemoryStorage<SiteDetailsVM> _fullStatusStorage;
    private readonly DatabaseStorage _databaseStorage;

    public SiteController(InMemoryStorage<SiteVM> liteStatusStorage, InMemoryStorage<SiteDetailsVM> fullStatusStorage, DatabaseStorage databaseStorage)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _databaseStorage = databaseStorage;
    }

    /// <summary>
    /// Returns all sites
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///    GET https://dev-russian-sites-status-api.herokuapp.com/api/sites
    ///
    /// </remarks>    
    /// <returns>List of sites</returns>
    /// <response code="200">List of sites</response>
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites")]
    public ActionResult<List<SiteVM>> GetAll()
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
    ///    GET https://dev-russian-sites-status-api.herokuapp.com/api/sites/2876347
    ///
    /// </remarks>    
    /// <param name="id">Site id, integer number</param>
    /// <returns>Site details</returns>    
    /// <response code="200">Site details</response>
    /// <response code="404">Site not found</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/{id}")]
    public ActionResult<SiteDetailsVM> Get(long id)
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
    ///    GET https://dev-russian-sites-status-api.herokuapp.com/api/sites/search?text=ya.ru
    ///
    /// </remarks>    
    /// <param name="text">Text, minimum 3 symbols</param>
    /// <returns>List of sites</returns>    
    /// <response code="200">List of sites</response>
    /// <response code="400">Bad request</response> 
    /// <response code="500">Internal server error</response> 
    [HttpGet("api/sites/search")]
    public ActionResult<IEnumerable<SiteDetailsVM>> Search([FromQuery] string text)
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
    ///    POST https://dev-russian-sites-status-api.herokuapp.com/api/sites/ya.ru
    ///
    /// </remarks>    
    /// <param name="siteUrl">Site URL to monitor</param>
    /// <response code="201">Created</response>
    /// <response code="400">Bad request</response> 
    /// <response code="401">Unauthorized</response> 
    /// <response code="409">Conflict</response> 
    /// <response code="500">Internal server error</response> 
    [HttpPost("api/sites/{siteUrl}")]
   // [Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
    public async Task<ActionResult> Add([FromRoute()]string siteUrl)
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

        var site = new Site
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
    ///    DELETE https://dev-russian-sites-status-api.herokuapp.com/api/sites/82197
    ///
    /// </remarks>    
    /// <param name="siteId">Site Id to remove</param>
    /// <response code="204">No content</response>
    /// <response code="401">Unauthorized</response> 
    /// <response code="404">Not found</response> 
    /// <response code="500">Internal server error</response> 
    [HttpDelete("api/sites/{siteId}")]
    // [Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
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
