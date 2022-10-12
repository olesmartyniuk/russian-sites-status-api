using TestProxy;

if (args.Length < 3)
{
    Console.WriteLine($"Params not found, default will be used. Example: 10 http://156.238.7.246:6258 https://www.google.com/");
    args = new string[3] { "10", "http://156.238.7.246:6258", "https://www.google.com/" };
}

var iterations = int.Parse(args[0]);
var proxy = args[1];
var url = args[2];

Console.WriteLine($"About to send {iterations} parallel requests via proxy '{proxy}' to '{url}'");
Console.WriteLine();

List<Result> results = await HttpCheck.RunChecks(iterations, proxy, url);

var ok = 0;
var proxyErrors = 0;
var siteErrors = 0;
var wholeTime = 0.0;

foreach (var result in results)
{
    switch (result.Code)
    {
        case ResultCode.Ok:
            ok++;
            break;
        case ResultCode.SiteError:
            siteErrors++;
            break;
        case ResultCode.ProxyError:
            proxyErrors++;
            break;
        default:
            break;
    }

    wholeTime += result.Time;
}

var averageTime = wholeTime / results.Count;

Console.WriteLine();
Console.WriteLine($"Proxy: {proxy}");
Console.WriteLine($"  ok - {ok}");
Console.WriteLine($"  site errors - {siteErrors}");
Console.WriteLine($"  proxy errors - {proxyErrors}");
Console.WriteLine($"  average time - {(int)averageTime} sec");
Console.WriteLine($"  whole time - {(int)wholeTime} sec");
Console.WriteLine();