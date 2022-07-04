
namespace RussianSitesStatus.Database.Models;


public class StatisticInfo
{
    public int hour { get; set; }

    public int up { get; set; }

    public int unknown { get; set; }

    public int down { get; set; }
}

public class SiteAgregateFor
{
    public long SiteId { get; set; }
    public DateTime AgregateFor { get; set; }
}
