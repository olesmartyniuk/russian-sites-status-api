using RussianSitesStatus.Extensions;
using RussianSitesStatus.Services;
using System.Globalization;

namespace RussianSitesStatus.BackgroundServices;

public class ArchiveWorker : BackgroundService
{
    private readonly ILogger<MonitorStatusWorker> _logger;
    private readonly CleanupChecksService _archiveService;
    private IConfiguration _configuration;

    public ArchiveWorker(
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
                await _archiveService.ArchiveOldData();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{nameof(ArchiveWorker)}: Unhandled exception: {e}");
            }            

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
