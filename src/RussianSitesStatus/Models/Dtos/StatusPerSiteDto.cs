using RussianSitesStatus.Database.Models;

namespace RussianSitesStatus.Models.Dtos;

public class StatusPerSiteDto
{
    public long SiteId { get; set; }
    public CheckStatus Status { get; set; }
}