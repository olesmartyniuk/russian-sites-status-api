using RussianSitesStatus.Extensions;
using RussianSitesStatus.Services;
using System.Globalization;

namespace RussianSitesStatus.BackgroundServices;

public class CleanupChecksWorker : BackgroundService
{
    private readonly ILogger<MonitorStatusWorker> _logger;
    private readonly CleanupChecksService _archiveService;
    private IConfiguration _configuration;

    public CleanupChecksWorker(
        ILogger<MonitorStatusWorker> logger,
        IConfiguration configuration,
        CleanupChecksService archiveService)
    {
        _logger = logger;
        _configuration = configuration;
        _archiveService = archiveService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!TimeSpan.TryParseExact(_configuration["ARCHIVE_AT"], "hh':'mm':'ss", CultureInfo.CurrentCulture, out TimeSpan archiveAt))
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(archiveAt.WaitTimeSpan(), stoppingToken);
            
            try
            {
                await _archiveService.CleanupOldData();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(CleanupChecksWorker)}: Unhandled exception: {e}");
            }            

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
