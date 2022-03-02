namespace RussianSitesStatus.Models;

public class Site
{
    public string Id { get; set; }
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

public class SiteDetails : Site
{
    public bool DoNotFind { get; set; }
    public List<Server> Servers { get; set; }
    public bool Processing { get; set; }
    public int Timeout { get; set; }
    public DateTime LastTestedAt { get; set; }
}

public class Server
{
    public string Description { get; set; }
    public string Region { get; set; }
    public string Status { get; set; }
}