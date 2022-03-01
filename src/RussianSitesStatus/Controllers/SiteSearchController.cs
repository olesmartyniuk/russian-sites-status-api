using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteSearchController : ControllerBase
{
    private readonly Storage<SiteDetails> _fullStatusStorage;

    public SiteSearchController(Storage<SiteDetails> fullStatusStorage)
    {
        _fullStatusStorage = fullStatusStorage;
    }

    [HttpGet("api/sites/search")]
    public ActionResult<IEnumerable<Site>> Search([FromQuery] string url, [FromQuery] PaginationFilter filter)
    {
        if (url?.Length < 2)
            return BadRequest($"The input url: {url} should contains more than 1 symbol");
        var result = _fullStatusStorage.Search(url, filter);
        return Ok(result);
    }
}