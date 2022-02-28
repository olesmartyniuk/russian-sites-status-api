namespace RussianSitesStatus.Extensions;

public static class UrlExtension
{
    public static string NormilizeStringUrl(this string stringUrl)
    {
        var uri = new Uri(stringUrl);
        return $"{uri.Scheme}://{uri.Host}";
    }

    public static bool IsValid(this string stringUrl)
    {
        return Uri.IsWellFormedUriString(stringUrl, UriKind.Absolute);
    }
}