using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Models;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
public class SiteStatusController : ControllerBase
{
    private readonly Storage<SiteStatus> _liteStatusStorage;
    private readonly Storage<SiteStatusFull> _fullStatusStorage;

    public SiteStatusController(Storage<SiteStatus> liteStatusStorage, Storage<SiteStatusFull> fullStatusStorage)
    {
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
    }

    [HttpGet("api/status")]
    public List<SiteStatus> GetAllStatuses()
    {
        return _liteStatusStorage
            .GetAll()
            .ToList();        
    }

    [HttpGet("api/status/{id}")]
    public ActionResult<SiteStatusFull> GetStatuse(string id)
    {
        var result = _fullStatusStorage.Get(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
