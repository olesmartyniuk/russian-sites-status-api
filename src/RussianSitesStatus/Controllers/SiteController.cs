using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteController : ControllerBase
{
    private readonly Storage<Site> _liteStatusStorage;
    private readonly Storage<SiteDetails> _fullStatusStorage;

    public SiteController(Storage<Site> liteStatusStorage, Storage<SiteDetails> fullStatusStorage)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
    }

    [HttpGet("api/sites")]
    public ActionResult<List<Site>> GetAllSites()
    {
        var result = _liteStatusStorage
            .GetAll()
            .ToList();

        return Ok(result);        
    }

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


    [HttpGet("api/sites/search")]
    public ActionResult<IEnumerable<Site>> Search([FromQuery] string text)
    {
        if (string.IsNullOrEmpty(text) | text.Length < 3)
        {
            return BadRequest("The search text should contains more than 2 symbols");
        }

        var result = _fullStatusStorage.Search(text);
        return Ok(result);
    }
}
