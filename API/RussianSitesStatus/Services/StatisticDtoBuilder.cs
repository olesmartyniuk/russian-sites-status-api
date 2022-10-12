using RussianSitesStatus.Database.Models;
using RussianSitesStatus.Models;
using System.Globalization;
using Site = RussianSitesStatus.Database.Models.Site;

namespace RussianSitesStatus.Services;

public static class StatisticDtoBuilder
{
    public static Models.Statistic GetForDay(IEnumerable<Statistic> statistic, DateTime periodStart, Site site)
    {
        var history = statistic.Select(s => new Span
        {
            Up = s.Up,
            Down = s.Down,
            Unknown = s.Unknown,
            Label = (s.Hour.Hour + 1).ToString()
        }).ToList();

        var periodEnd = periodStart.AddDays(1);

        var index = 0;
        for (var date = periodStart; date <= periodEnd; date = date.AddHours(1))
        {
            var label = (date.Hour + 1).ToString();
            if (history.Exists(d => d.Label == label) == false)
            {
                history.Insert(index, new Span
                {
                    Label = label
                });
            }

            index++;
        }

        var result = new Models.Statistic
        {
            Navigation = GetNavigation(site, periodStart, PeriodType.Day),
            Periods = GetPeriods(site, PeriodType.Day),
            Data = GetData(history)
        };

        return result;
    }

    private static Data GetData(List<Span> history)
    {
        double up = history.Sum(s => s.Up);
        double all = history.Sum(s => s.Up + s.Down + s.Unknown);


        int? uptime = all == 0 ? 
            null : 
            (int)Math.Round(up / all * 100);

        return new Data
        {
            History = history.ToList(),
            Uptime = uptime
        };
    }

    public static Models.Statistic GetForWeek(IEnumerable<Statistic> statistic, DateTime periodStart, Site site)
    {
        var history = statistic.GroupBy(
           stat => stat.Hour.DayOfWeek,
           (dayOfWeek, stats) => new Span
           {
               Label = dayOfWeek.ToString(),
               Up = stats.Sum(stat => stat.Up),
               Down = stats.Sum(stat => stat.Down),
               Unknown = stats.Sum(stat => stat.Unknown)
           }
        ).ToList();

        var periodEnd = periodStart.AddDays(7);

        var index = 0;
        for (var date = periodStart; date <= periodEnd; date = date.AddDays(1))
        {
            var label = date.DayOfWeek.ToString();
            if (history.Exists(d => d.Label == label) == false)
            {
                history.Insert(index, new Span
                {
                    Label = label                    
                });
            }

            index++;
        }

        var result = new Models.Statistic
        {
            Navigation = GetNavigation(site, periodStart, PeriodType.Week),
            Periods = GetPeriods(site, PeriodType.Week),
            Data = GetData(history)
        };

        return result;
    }

    public static Models.Statistic GetForMonth(IEnumerable<Statistic> statistic, DateTime periodStart, Site site)
    {
        var history = statistic.GroupBy(
           stat => stat.Hour.Day,
           (day, stats) => new Span
           {
               Label = day.ToString(),
               Up = stats.Sum(stat => stat.Up),
               Down = stats.Sum(stat => stat.Down),
               Unknown = stats.Sum(stat => stat.Unknown)
           }
       ).ToList();

        var periodEnd = periodStart.AddMonths(1);

        var index = 0;
        for (var date = periodStart; date <= periodEnd; date = date.AddDays(1))
        {
            var label = date.Day.ToString();
            if (history.Exists(d => d.Label == label) == false)
            {
                history.Insert(index, new Span
                {
                    Label = label
                });
            }

            index++;
        }

        var result = new Models.Statistic
        {
            Navigation = GetNavigation(site, periodStart, PeriodType.Month),
            Periods = GetPeriods(site, PeriodType.Month),
            Data = GetData(history)
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
                Name = currentDate.ToString("MMMM dd, hh:mm", CultureInfo.GetCultureInfo("en-US")),
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
                Name = currentDate.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US")),
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
                Name = $"Week {GetWeekNumber(currentDate)}",
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
                Name = currentDate.ToString("MMMM", CultureInfo.GetCultureInfo("en-US")),
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
