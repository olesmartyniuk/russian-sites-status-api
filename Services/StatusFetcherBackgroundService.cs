using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services;

public class StatusFetcherBackgroundService : BackgroundService
{
    private const int WAIT_TO_NEXT_CHECK_SECONDS = 15;
    private readonly StatusCakeService _statusCakeService;
    private readonly Storage<SiteStatus> _liteStatusStorage;
    private readonly Storage<SiteStatusFull> _fullStatusStorage;
    private readonly ILogger<StatusFetcherBackgroundService> _logger;
    public StatusFetcherBackgroundService(
        StatusCakeService statusCakeService,
        Storage<SiteStatus> liteStatusStorage,
        Storage<SiteStatusFull> fullStatusStorage,
        ILogger<StatusFetcherBackgroundService> logger)
    {
        _statusCakeService = statusCakeService;
        _liteStatusStorage = liteStatusStorage;
        _fullStatusStorage = fullStatusStorage;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var statuses = await UpdateLiteStatuses();
                await UpdateFullStatuses(statuses);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception while fetching statuses");
            }

            await Task.Delay(TimeSpan.FromSeconds(WAIT_TO_NEXT_CHECK_SECONDS), stoppingToken);
        }
    }

    private async Task UpdateFullStatuses(IEnumerable<SiteStatus> statuses)
    {
        foreach (var status in statuses)
        {
            var uptimeCheck = await _statusCakeService.GetStatus(status.Id);
            var data = uptimeCheck.data;
            var siteStatus = new SiteStatusFull
            {
                Id = data.id,
                Name = data.name,
                Status = data.status,
                TestType = data.test_type,
                Uptime = $"{data.uptime} %",
                WebsiteUrl = data.website_url,
                DoNotFind = data.do_not_find,
                LastTestedAt = data.last_tested_at,
                Processing = data.processing,
                Servers = data.servers.Select(s => new Server
                {
                    Description = s.description,
                    Ipv4 = s.ipv4,
                    Region = s.region,
                    Status = s.status
                }).ToList(),
                Timeout = data.timeout
            };

            _fullStatusStorage.Replace(siteStatus);
        }
    }

    private async Task<IEnumerable<SiteStatus>> UpdateLiteStatuses()
    {
        var statuses = await _statusCakeService.GetAllStatuses();

        var siteStatuses = statuses.data.Select(status => new SiteStatus
        {
            Id = status.id,
            Name = status.name,
            Status = status.status,
            TestType = status.test_type,
            Uptime = $"{status.uptime} %",
            WebsiteUrl = status.website_url
        });

        _liteStatusStorage.ReplaceAll(siteStatuses);

        return siteStatuses;
    }
}