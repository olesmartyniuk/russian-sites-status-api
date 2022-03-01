using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteSearchController : ControllerBase
{
    private readonly Storage<Site> _liteStatusStorage;

    public SiteSearchController(Storage<Site> liteStatusStorage)
    {
        _liteStatusStorage = liteStatusStorage;
    }

    [HttpGet("api/sites/search")]
    public ActionResult<IEnumerable<Site>> Search([FromQuery] string url, [FromQuery] PaginationFilter filter)
    {
        if (url.Length < 2)
            return BadRequest($"The input url: {url} should contains more than 1 symbol");
        var result = _liteStatusStorage.Search(url, filter);
        return Ok(result);
    }
}