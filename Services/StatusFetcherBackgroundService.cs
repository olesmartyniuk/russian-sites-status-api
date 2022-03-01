using RussianSitesStatus.Models;

namespace RussianSitesStatus.Services;

public class StatusFetcherBackgroundService : BackgroundService
{
    private const int WAIT_TO_NEXT_CHECK_SECONDS = 15;
    private readonly StatusCakeService _statusCakeService;
    private readonly Storage<Site> _liteStatusStorage;
    private readonly Storage<SiteDetails> _fullStatusStorage;
    private readonly ILogger<StatusFetcherBackgroundService> _logger;

    public StatusFetcherBackgroundService(
        StatusCakeService statusCakeService,
        Storage<Site> liteStatusStorage,
        Storage<SiteDetails> fullStatusStorage,
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

    private async Task UpdateFullStatuses(IEnumerable<Site> statuses)
    {
        foreach (var status in statuses)
        {
            var uptimeCheck = await _statusCakeService.GetStatus(status.Id);
            var data = uptimeCheck.data;
            var siteStatus = new SiteDetails
            {
                Id = data.id,
                Name = NormalizeName(data.name),
                Status = data.status,
                TestType = data.test_type,
                Uptime = data.uptime,
                WebsiteUrl = data.website_url,
                DoNotFind = data.do_not_find,
                LastTestedAt = data.last_tested_at,
                Processing = data.processing,
                Servers = data.servers.Select(s => new Server
                {
                    Description = s.description,
                    Region = s.region,
                    Status = s.status
                }).ToList(),
                Timeout = data.timeout
            };

            _fullStatusStorage.Replace(siteStatus);
        }
    }

    private async Task<IEnumerable<Site>> UpdateLiteStatuses()
    {
        var statuses = await _statusCakeService.GetAllStatuses();

        var siteStatuses = statuses.data.Select(status => new Site
        {
            Id = status.id,
            Name = NormalizeName(status.name),
            Status = status.status,
            TestType = status.test_type,
            Uptime = status.uptime,
            WebsiteUrl = status.website_url
        });

        _liteStatusStorage.ReplaceAll(siteStatuses);

        return siteStatuses;
    }

    private string NormalizeName(string name)
    {
        if (name.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Replace("http://", string.Empty);
        }

        if (name.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Replace("https://", string.Empty);
        }

        if (name.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
        {
            name = name.Replace("www.", string.Empty);
        }

        if (name.EndsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            name = name.TrimEnd('/');
        }

        return name;
    }
}