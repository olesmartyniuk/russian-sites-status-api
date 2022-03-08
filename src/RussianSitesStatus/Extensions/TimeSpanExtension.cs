namespace RussianSitesStatus.Extensions;

public static class TimeSpanExtension
{
    private static readonly int SECONDS_IN_ONE_DAY = 24 * 60 * 60;

    public static TimeSpan WaitTimeSpan(this TimeSpan calculateAt)
    {
        var timeDiffInSeconds = (int)Math.Abs(DateTime.UtcNow.TimeOfDay.TotalSeconds - calculateAt.TotalSeconds);
        if (DateTime.UtcNow.TimeOfDay.TotalSeconds > calculateAt.TotalSeconds)
        {
            return TimeSpan.FromSeconds(SECONDS_IN_ONE_DAY - timeDiffInSeconds);
        }

        return TimeSpan.FromSeconds(timeDiffInSeconds);
    }
}