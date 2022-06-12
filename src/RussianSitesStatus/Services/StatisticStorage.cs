using Newtonsoft.Json;
using RussianSitesStatus.Database.Models;

namespace RussianSitesStatus.Services
{
    public class StatisticStorage
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Dictionary<long, List<Statistic>> _storage = new();
        private DateTime _lastUpdated = default;

        public StatisticStorage(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IEnumerable<Statistic> GetData(Site site, DateTime periodStart, DateTime periodEnd)
        {
            if (!_storage.ContainsKey(site.Id))
            {
                return Enumerable.Empty<Statistic>();
            }

            var statistics = _storage[site.Id];

            return statistics
                .Where(s => periodStart <= s.Hour && s.Hour < periodEnd);
        }

        public async Task UpdateStorage()
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var database = serviceScope.ServiceProvider.GetRequiredService<DatabaseStorage>();

            var checkStats = await database.GetStatistics(_lastUpdated);

            foreach (var checkStat in checkStats)
            {
                if (!_storage.ContainsKey(checkStat.SiteId))
                {
                    _storage[checkStat.SiteId] = new List<Statistic>();
                }

                var statisticList = _storage[checkStat.SiteId];
                var dataByHours = JsonConvert.DeserializeObject<List<StatisticInfo>>(checkStat.Data);

                foreach (var data in dataByHours)
                {
                    var statisticItem = new Statistic
                    {
                        Hour = checkStat.Day.AddHours(data.hour),
                        Down = data.down,
                        Up = data.up,
                        Unknown = data.unknown
                    };
                    statisticList.Add(statisticItem);
                };
            }

            _lastUpdated = DateTime.UtcNow;
        }
    }

    public class Statistic
    {
        public DateTime Hour { get; set; }
        public int Up { get; set; }
        public int Down { get; set; }
        public int Unknown { get; set; }
    }
}
