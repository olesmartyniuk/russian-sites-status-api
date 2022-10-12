using System.Diagnostics;
using System.Net;

namespace TestProxy;

internal static class HttpCheck
{
    public static async Task<List<Result>> RunChecks(int iterations, string proxy, string url)
    {
        var tasks = new List<Task<Result>>();
        for (int i = 0; i < iterations; i++)
        {
            var task = Task.Run(async () => await MakeCallAndReportResult(url, proxy));
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        return tasks.Select(t => t.Result).ToList();
    }

    private static async Task<Result> MakeCallAndReportResult(string url, string proxyUrl)
    {
        var watch = new Stopwatch();
        try
        {
            var proxy = new WebProxy
            {
                Address = new Uri(proxyUrl),
                BypassProxyOnLocal = false,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(@"zbcffrty", "mf3975nu0qj6")
            };

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = true,
            };

            var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            client.Timeout = TimeSpan.FromSeconds(30);

            watch.Start();

            var response = await client.GetAsync(url);

            watch.Stop();

            return new Result
            {
                Code = ResultCode.Ok,
                Time = (double)watch.ElapsedMilliseconds / 1000
            };
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Proxy error: {e.Message}");
            return new Result
            {
                Code = ResultCode.ProxyError,
                Time = (double)watch.ElapsedMilliseconds / 1000
            };
        }

        catch (Exception e)
        {
            Console.WriteLine($"Unknown error: {e.Message}");
            return new Result
            {
                Code = ResultCode.ProxyError,
                Time = (double)watch.ElapsedMilliseconds / 1000
            };
        }
    }
}

internal enum ResultCode
{
    Ok,
    SiteError,
    ProxyError
}

internal class Result
{
    public double Time { get; set; }
    public ResultCode Code { get; set; }
}
