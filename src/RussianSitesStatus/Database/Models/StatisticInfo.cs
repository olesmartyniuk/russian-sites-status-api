
namespace RussianSitesStatus.Database.Models;


public class StatisticInfo
{
    public long region { get; set; }

    public int hour { get; set; }

    public double avg_time { get; set; }

    public int up_number { get; set; }

    public int unavailable_number { get; set; }

    public int down_number { get; set; }
}

public class SiteAgregateFor
{
    public long SiteId { get; set; }
    public DateTime AgregateFor { get; set; }
}
