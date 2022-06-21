namespace RussianSitesStatus.Models;

public class Statistic
{
    public Navigation Navigation { get; set; }
    public List<Period> Periods { get; set; }
    public List<Data> Data { get; set; }
}

public class Data
{
    public int Up { get; set; }
    public int Down { get; set; }
    public int Unknown { get; set; }
    public string Label { get; set; }
}

public class Period
{
    public string Name { get; set; }
    public bool Current { get; set; }
    public string Url { get; set; }
}

public class Navigation
{
    public Link Current { get; set; }
    public Link Next { get; set; }
    public Link Prev { get; set; }
}

public class Link
{
    public string Name { get; set; }
    public string Url { get; set; }
}

public enum PeriodType
{
    Hour, 
    Day, 
    Week, 
    Month
}
