namespace RussianSitesStatus.Configuration
{
    public class MonitorSitesConfiguration
    {
        public int WaitToNextCheckSeconds { get; set; }
        public int WaitBeforeFirstIterationSeconds { get; set; }
        public int Rate { get; set; }
    }
}