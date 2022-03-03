namespace RussianSitesStatus.Models.StatusCake;

public class UptimeCheckServer
{
    public string description { get; set; }
    public string region { get; set; }
    public string region_code { get; set; }
    public string status { get; set; }
    public string ipv4 { get; set; }
}

public class UptimeCheckItem
{
    public string id { get; set; }
    public bool paused { get; set; }
    public string name { get; set; }
    public string test_type { get; set; }
    public string website_url { get; set; }
    public int check_rate { get; set; }
    public int confirmation { get; set; }
    public List<object> contact_groups { get; set; }
    public bool do_not_find { get; set; }
    public bool enable_ssl_alert { get; set; }
    public bool follow_redirects { get; set; }
    public bool include_header { get; set; }
    public List<UptimeCheckServer> servers { get; set; }
    public bool processing { get; set; }
    public string status { get; set; }
    public List<string> tags { get; set; }
    public int timeout { get; set; }
    public int trigger_rate { get; set; }
    public double uptime { get; set; }
    public bool use_jar { get; set; }
    public DateTime last_tested_at { get; set; }
    public string next_location { get; set; }
    public List<string> status_codes { get; set; }
    public List<string> regions { get; set; }
}

public class UptimeCheck
{
    public UptimeCheckItem data { get; set; }
}