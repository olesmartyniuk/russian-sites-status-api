using RussianSitesStatus.Models;
using RussianSitesStatus.Services;

namespace RussianSitesStatus.BackgroundServices;

public class StatisticDataFetcher : BackgroundService
{
    private readonly StatisticStorage _statisticStorage;
    private readonly ILogger<MemoryDataFetcher> _logger;    
    private readonly IConfiguration _configuration;
    private readonly int _memoryDataSyncInterval;

    public StatisticDataFetcher(
        StatisticStorage statisticStorage,
        ILogger<MemoryDataFetcher> logger,
        IConfiguration configuration
       )
    {
        _statisticStorage = statisticStorage;
        _logger = logger;
        _configuration = configuration;

        _memoryDataSyncInterval = int.Parse(_configuration["MEMORY_DATA_SYNC_INTERVAL"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {        
        if (_memoryDataSyncInterval <= 0)
        {
            _logger.LogInformation($"StatisticyDataFetcher will not start, MEMORY_DATA_SYNC_INTERVAL={_memoryDataSyncInterval}.");
            return;
        }

        _logger.LogInformation($"StatisticyDataFetcher started, MEMORY_DATA_SYNC_INTERVAL={_memoryDataSyncInterval}.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateStatisticInStorage();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while synchronized data from DB.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_memoryDataSyncInterval), stoppingToken);
        }
    }

    private async Task UpdateStatisticInStorage()
    {
        await _statisticStorage.UpdateStorage();

        _logger.LogInformation($"All in-memory storages were updated.");
    }
}