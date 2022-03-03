namespace RussianSitesStatus.Configuration
{
    public class MonitorSitesConfiguration
    {
        public int WaitToNextCheckSeconds { get; set; }
        public int WaitBeforeFirstIterationSeconds { get; set; }
    }
}