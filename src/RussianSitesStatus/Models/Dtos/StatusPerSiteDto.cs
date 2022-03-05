namespace RussianSitesStatus.Models.Dtos;

public enum SiteStatus
{
    Down = 0,
    Up = 1,
    FailToIdentify = 2,
}

public class StatusPerSiteDto
{
    public long SiteId { get; set; }
    public SiteStatus Status { get; set; }
}