namespace RussianSitesStatus.Models;

public class UptimeCheckHistory
{
    public List<UptimeCheckHistoryItem> data { get; set; }
}

public class UptimeCheckHistoryItem
{
    public DateTime created_at { get; set; }
    public int status_code { get; set; }
    public string location { get; set; }
    public int performance { get; set; }
}    
