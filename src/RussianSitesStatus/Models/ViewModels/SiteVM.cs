namespace RussianSitesStatus.Models;

public class SiteVM
{
    public string Id { get; set; } //TODO: Why string?
    public string Name { get; set; }
    public string TestType { get; set; }
    public string WebsiteUrl { get; set; }
    public string Status { get; set; }
    public double Uptime { get; set; }
    public List<string> Tags { get; set; }


    internal object Where()
    {
        throw new NotImplementedException();
    }
}

public class SiteDetailsVM : SiteVM
{
    public List<ServerDto> Servers { get; set; }
    public int Timeout { get; set; }
    public DateTime LastTestedAt { get; set; }
}

public class ServerDto
{    
    public string Region { get; set; }
    public string RegionCode { get; set; }
    public string Status { get; set; }
    public int StatusCode { get; internal set; }
    public DateTime LastTestedAt { get; set; }    
}