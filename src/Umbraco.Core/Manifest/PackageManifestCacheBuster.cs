using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Appends cache-busting query parameters to package importmap asset URLs. Extension asset URLs are stamped
///     client-side instead (the backoffice reads each package's <c>version</c> off the manifest); the
///     <c>%CACHE_BUSTER%</c> token is resolved separately, server-side.
/// </summary>
public static class PackageManifestCacheBuster
{
    // The /App_Plugins root with a trailing slash, so the StartsWith check honours a path-segment boundary
    // (and never matches e.g. "/App_PluginsFoo/...").
    private static readonly string _appPluginsPrefix = Constants.SystemDirectories.AppPlugins.EnsureEndsWith('/');

    /// <summary>
    ///     When <paramref name="autoStamp"/> is <c>true</c>, appends
    ///     <c>?v=&lt;version&gt;&amp;umb__rnd=&lt;cacheBuster&gt;</c> (only the values present) to a clean
    ///     <c>/App_Plugins</c> URL. Everything else is returned unchanged.
    /// </summary>
    public static string ApplyCacheBust(string url, string? version, string? cacheBuster, bool autoStamp)
    {
        if (string.IsNullOrEmpty(url) || autoStamp is false)
        {
            return url;
        }

        // Only auto-stamp the package's own /App_Plugins assets, and never a URL that already manages its own query.
        if (url.StartsWith(_appPluginsPrefix, StringComparison.OrdinalIgnoreCase) is false || url.Contains('?'))
        {
            return url;
        }

        var query = BuildQuery(version, cacheBuster);
        if (query.Length == 0)
        {
            return url;
        }

        var fragmentIndex = url.IndexOf('#');
        return fragmentIndex < 0
            ? $"{url}?{query}"
            : $"{url[..fragmentIndex]}?{query}{url[fragmentIndex..]}";
    }

    private static string BuildQuery(string? version, string? cacheBuster)
    {
        var parameters = new List<string>(2);
        if (string.IsNullOrWhiteSpace(version) is false)
        {
            parameters.Add($"v={Uri.EscapeDataString(version)}");
        }

        if (string.IsNullOrWhiteSpace(cacheBuster) is false)
        {
            parameters.Add($"umb__rnd={Uri.EscapeDataString(cacheBuster)}");
        }

        return string.Join('&', parameters);
    }
}
