using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using System.Globalization;

namespace RussianSitesStatus.Services;

public static class StatisticViewModelHelper
{
    public static StatisticVm GetForDay(IEnumerable<Statistic> statistic, DateTime periodStart, Site site)
    {
        var data = statistic.Select(s => new DataItem
        {
            Up = s.Up,
            Down = s.Down,
            Unknown = s.Unknown,
            Label = s.Hour.Hour.ToString()
        });

        var result = new StatisticVm
        {
            Navigation = GetNavigation(site, periodStart, PeriodType.Day),
            Periods = GetPeriods(site, PeriodType.Day),
            Data = data.ToList()
        };

        return result;
    }

    public static StatisticVm GetForWeek(IEnumerable<Statistic> statistic, DateTime periodStart, Site site)
    {
        var data = statistic.GroupBy(
           stat => stat.Hour.DayOfWeek,
           (dayOfWeek, stats) => new DataItem
           {
               Label = dayOfWeek.ToString(),
               Up = stats.Sum(stat => stat.Up),
               Down = stats.Sum(stat => stat.Down),
               Unknown = stats.Sum(stat => stat.Unknown)
           }
       ).ToList();

        var currentDate = periodStart;
        var nextDate = periodStart.AddDays(7);
        var prevDate = periodStart.AddDays(-7);

        var result = new StatisticVm
        {
            Navigation = GetNavigation(site, currentDate, PeriodType.Week),
            Periods = GetPeriods(site, PeriodType.Week),
            Data = data
        };

        return result;
    }

    public static StatisticVm GetForMonth(IEnumerable<Statistic> statistic, DateTime periodStart, Site site)
    {
        var data = statistic.GroupBy(
           stat => stat.Hour.Day,
           (day, stats) => new DataItem
           {
               Label = day.ToString(),
               Up = stats.Sum(stat => stat.Up),
               Down = stats.Sum(stat => stat.Down),
               Unknown = stats.Sum(stat => stat.Unknown)
           }
       ).ToList();

        var currentDate = periodStart;
        var nextDate = periodStart.AddMonths(1);
        var prevDate = periodStart.AddMonths(-1);

        var result = new StatisticVm
        {
            Navigation = GetNavigation(site, currentDate, PeriodType.Month),
            Periods = GetPeriods(site, PeriodType.Month),
            Data = data
        };

        return result;
    }

    private static List<Period> GetPeriods(Site site, PeriodType period)
    {
        var periodHour = new Period
        {
            Current = period == PeriodType.Hour,
            Name = "Hour",
            Url = $"api/sites/{site.Id}/statistics/period/hour/date/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}/{DateTime.UtcNow.Hour}"
        };
        var periodDay = new Period
        {
            Current = period == PeriodType.Day,
            Name = "Day",
            Url = $"api/sites/{site.Id}/statistics/period/day/date/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}/{DateTime.UtcNow.Day}"
        };
        var periodWeek = new Period
        {
            Current = period == PeriodType.Week,
            Name = "Week",
            Url = $"api/sites/{site.Id}/statistics/period/week/date/{DateTime.UtcNow.Year}/{GetWeekNumber(DateTime.UtcNow)}"
        };
        var periodMonth = new Period
        {
            Current = period == PeriodType.Month,
            Name = "Month",
            Url = $"api/sites/{site.Id}/statistics/period/month/date/{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month}"
        };

        return new List<Period> { periodHour, periodDay, periodWeek, periodMonth };
    }

    private static Navigation GetNavigation(Site site, DateTime periodStart, PeriodType period)
    {
        return period switch
        {
            PeriodType.Hour => GetNavigationForHour(site, periodStart),
            PeriodType.Day => GetNavigationForDay(site, periodStart),
            PeriodType.Week => GetNavigationForWeek(site, periodStart),
            PeriodType.Month => GetNavigationForMonth(site, periodStart),
            _ => throw new NotImplementedException()
        };
    }

    private static Navigation GetNavigationForHour(Site site, DateTime periodStart)
    {
        string FormatUrl(DateTime date)
        {
            return $"api/sites/{site.Id}/statistics/period/hour/date/{date.Year}/{date.Month}/{date.Day}/{date.Hour}";
        }

        var currentDate = periodStart;
        var nextDate = periodStart.AddHours(1);
        var prevDate = periodStart.AddHours(-1);

        return new Navigation
        {
            Current = new Link
            {
                Name = "Current",
                Url = FormatUrl(currentDate)
            },
            Next = new Link
            {
                Name = "Next",
                Url = FormatUrl(nextDate)
            },
            Prev = new Link
            {
                Name = "Prev",
                Url = FormatUrl(prevDate)
            }
        };
    }

    private static Navigation GetNavigationForDay(Site site, DateTime periodStart)
    {
        string FormatUrl(DateTime date)
        {
            return $"api/sites/{site.Id}/statistics/period/day/date/{date.Year}/{date.Month}/{date.Day}";
        }

        var currentDate = periodStart;
        var nextDate = periodStart.AddDays(1);
        var prevDate = periodStart.AddDays(-1);

        return new Navigation
        {
            Current = new Link
            {
                Name = "Current",
                Url = FormatUrl(currentDate)
            },
            Next = new Link
            {
                Name = "Next",
                Url = FormatUrl(nextDate)
            },
            Prev = new Link
            {
                Name = "Prev",
                Url = FormatUrl(prevDate)
            }
        };
    }

    private static Navigation GetNavigationForWeek(Site site, DateTime periodStart)
    {
        string FormatUrl(DateTime date)
        {
            return $"api/sites/{site.Id}/statistics/period/week/date/{date.Year}/{GetWeekNumber(date)}";
        }

        var currentDate = periodStart;
        var nextDate = periodStart.AddDays(7);
        var prevDate = periodStart.AddDays(-7);

        return new Navigation
        {
            Current = new Link
            {
                Name = "Current",
                Url = FormatUrl(currentDate)
            },
            Next = new Link
            {
                Name = "Next",
                Url = FormatUrl(nextDate)
            },
            Prev = new Link
            {
                Name = "Prev",
                Url = FormatUrl(prevDate)
            }
        };
    }

    private static Navigation GetNavigationForMonth(Site site, DateTime periodStart)
    {
        string FormatUrl(DateTime date)
        {
            return $"api/sites/{site.Id}/statistics/period/month/date/{date.Year}/{date.Month}";
        }

        var currentDate = periodStart;
        var nextDate = periodStart.AddMonths(1);
        var prevDate = periodStart.AddMonths(-1);

        return new Navigation
        {
            Current = new Link
            {
                Name = "Current",
                Url = FormatUrl(currentDate)
            },
            Next = new Link
            {
                Name = "Next",
                Url = FormatUrl(nextDate)
            },
            Prev = new Link
            {
                Name = "Prev",
                Url = FormatUrl(prevDate)
            }
        };
    }

    private static int GetWeekNumber(DateTime date)
    {
        var calendar = CultureInfo.CurrentCulture.Calendar;

        return calendar.GetWeekOfYear(
            date,
            CalendarWeekRule.FirstFullWeek,
            DayOfWeek.Monday);
    }
}
