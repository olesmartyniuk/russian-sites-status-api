namespace RussianSitesStatus.Extensions;

public static class StringExtension
{
    public static string NormalizeSiteName(this string name)
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

    public static string NormalizeSiteUrl(this string url)
    {
        return new UriBuilder(url)
            .Uri
            .ToString()
            .ToLower();
    }
}