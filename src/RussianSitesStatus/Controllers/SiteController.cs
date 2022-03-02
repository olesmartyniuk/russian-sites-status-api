using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RussianSitesStatus.Auth;
using RussianSitesStatus.Models.Constants;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = Scheme.ApiKeyAuthScheme)]
[Route("api/[controller]")]
public class SiteController : ControllerBase
{
    private readonly UpCheckService upCheckService;

    public SiteController(UpCheckService upCheckService)
    {
        this.upCheckService = upCheckService;
    }

    [HttpPost("add")]
    public async Task AddNewSiteAsync(string siteUrl)
    {
        await upCheckService.AddUptimeCheckAsync(siteUrl, new List<string> { Tag.CustomSite });
    }
}
