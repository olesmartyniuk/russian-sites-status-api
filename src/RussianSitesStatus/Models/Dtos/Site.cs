namespace RussianSitesStatus.Models;

public class SiteVM
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public double? Uptime { get; set; }
}

public class SiteDetailsVM : SiteVM
{
    public List<ServerDto> Servers { get; set; }    
    public DateTime LastTestedAt { get; set; }
}

public class ServerDto
{
    public string Region { get; set; }
    public string RegionCode { get; set; }
    public string Status { get; set; }
    public int StatusCode { get; internal set; }
}