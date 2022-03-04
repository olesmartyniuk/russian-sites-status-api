namespace RussianSitesStatus.Models;
public class UptimeChecksItem
{
    public string id { get; set; }
    public bool paused { get; set; }
    public string name { get; set; }
    public string website_url { get; set; }
    public string test_type { get; set; }
    public int check_rate { get; set; }
    public List<object> contact_groups { get; set; }
    public string status { get; set; }
    public List<string> tags { get; set; }
    public double uptime { get; set; }
}

public class Metadata
{
    public int page { get; set; }
    public int per_page { get; set; }
    public int page_count { get; set; }
    public int total_count { get; set; }
}

public class UptimeChecks
{
    public List<UptimeChecksItem> data { get; set; }
    public Metadata metadata { get; set; }
}