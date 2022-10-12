using RussianSitesStatus.Models.ViewModels;

namespace RussianSitesStatus.Models;

public class Site: BaseModel
{    
    public string Name { get; set; }
    public string Status { get; set; }
    public double? Uptime { get; set; }
}

public class SiteDetails : Site
{
    public List<Server> Servers { get; set; }    
    public DateTime LastTestedAt { get; set; }
}

public class Server
{
    public string Region { get; set; }
    public string RegionCode { get; set; }
    public string Status { get; set; }
    public int StatusCode { get; internal set; }
    public int SpentTimeInSec { get; internal set; }
}