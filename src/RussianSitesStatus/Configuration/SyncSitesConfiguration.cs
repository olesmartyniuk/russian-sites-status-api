namespace RussianSitesStatus.Configuration
{
    public class SyncSitesConfiguration
    {
        public int WaitToNextCheckSeconds { get; set; }
        public int WaitBeforeFirstIterationSeconds { get; set; }
    }
}