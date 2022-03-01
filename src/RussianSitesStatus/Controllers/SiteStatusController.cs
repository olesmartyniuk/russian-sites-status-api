using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteStatusController : ControllerBase
{
    private readonly Storage<Site> _liteStatusStorage;
    private readonly Storage<SiteDetails> _fullStatusStorage;

    public SiteStatusController(Storage<Site> liteStatusStorage, Storage<SiteDetails> fullStatusStorage)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
    }

    [HttpGet("api/status")]
    public List<Site> GetAllStatuses()
    {
        return _liteStatusStorage
            .GetAll()
            .ToList();        
    }

    [HttpGet("api/status/{id}")]
    public ActionResult<SiteDetails> GetStatuse(string id)
    {
        var result = _fullStatusStorage.Get(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
