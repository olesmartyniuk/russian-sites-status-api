using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Auth;
using RussianSitesStatus.Models;
using RussianSitesStatus.Models.Constants;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteController : ControllerBase
{
    private readonly Storage<Site> _liteStatusStorage;
    private readonly Storage<SiteDetails> _fullStatusStorage;
    private readonly UpCheckService _upCheckService;

    public SiteController(Storage<Site> liteStatusStorage, Storage<SiteDetails> fullStatusStorage, UpCheckService upCheckService)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _upCheckService = upCheckService;
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
    public ActionResult<List<Site>> GetAllSites()
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
    public ActionResult<SiteDetails> GetSite(string id)
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
    /// <response code="401">Unauthorized</response> 
    /// <response code="500">Internal server error</response> 
    [HttpPost("api/sites/{siteUrl}")]
    [Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
    public async Task<ActionResult> AddNewSiteAsync([FromRoute]string siteUrl)
    {
        await _upCheckService.AddUptimeCheckAsync(siteUrl, new List<string> { Tag.CustomSite });

        return Created("", "");
    }
}
