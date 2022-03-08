using RussianSitesStatus.Extensions;
using RussianSitesStatus.Services;
using System.Diagnostics;

namespace RussianSitesStatus.BackgroundServices;

public class ArchiveWorker : BackgroundService
{
    private readonly ILogger<MonitorStatusWorker> _logger;
    private readonly ArchiveService _archiveService;
    private IConfiguration _configuration;

    public ArchiveWorker(
        ILogger<MonitorStatusWorker> logger,
        IConfiguration configuration,
        ArchiveService archiveService)
    {
        _logger = logger;
        _configuration = configuration;
        _archiveService = archiveService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!TimeSpan.TryParse(_configuration["ARCHIVE_AT"], out TimeSpan archiveAt))
        {
            return;
        }

        var spentTime = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(archiveAt.WaitTimeSpan(), stoppingToken);
            try
            {
                var timer = new Stopwatch();
                timer.Start();

                await _archiveService.ArchiveOldDataAsync();

                timer.Stop();
                spentTime = (int)timer.Elapsed.TotalSeconds;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }
            _logger.LogInformation($"{nameof(ArchiveWorker)}: executed iteration in {spentTime} seconds.");

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
