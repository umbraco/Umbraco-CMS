using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Applies cache-busting query parameters to package manifest asset URLs.
/// </summary>
/// <remarks>
///     This is used server-side for the type-safe importmap only. Registered extension asset URLs are cache-busted
///     client-side (the backoffice reads each package's <c>version</c> off the manifest and applies the same rules),
///     so the server never has to unwrap the untyped extension JSON.
/// </remarks>
public static class PackageManifestCacheBuster
{
    // The /App_Plugins root with a trailing slash, so the StartsWith check honours a path-segment boundary
    // (and never matches e.g. "/App_PluginsFoo/...").
    private static readonly string _appPluginsPrefix = Constants.SystemDirectories.AppPlugins.EnsureEndsWith('/');

    /// <summary>
    ///     Applies cache-busting to a single manifest URL.
    ///     <para>
    ///     An explicit <c>%CACHE_BUSTER%</c> token is always resolved (wherever it appears) to the package
    ///     <paramref name="version"/>, falling back to the host <paramref name="cacheBuster"/> — that is the author's
    ///     deliberate opt-in. Otherwise, when <paramref name="autoStamp"/> is <c>true</c>, a clean
    ///     <c>/App_Plugins</c>-rooted URL gets <c>?v=&lt;version&gt;&amp;umb__rnd=&lt;cacheBuster&gt;</c> appended (only
    ///     the values that are present). Everything else — the backoffice core, CDNs, protocol-relative URLs, bare
    ///     module specifiers, relative paths, and URLs that already carry a query string — is returned unchanged.
    ///     </para>
    /// </summary>
    public static string ApplyCacheBust(string url, string? version, string? cacheBuster, bool autoStamp)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        // The explicit %CACHE_BUSTER% opt-in: resolve it wherever the author placed it, regardless of autoStamp.
        // A URL the author already tokenised is never also auto-stamped.
        if (url.Contains(Constants.Web.CacheBusterToken, StringComparison.Ordinal))
        {
            return url.Replace(Constants.Web.CacheBusterToken, version ?? cacheBuster ?? string.Empty, StringComparison.Ordinal);
        }

        if (autoStamp is false)
        {
            return url;
        }

        // Automatic stamping only ever touches the package's own /App_Plugins assets. This also excludes the backoffice
        // core (/umbraco/backoffice/...), CDNs, protocol-relative URLs, bare specifiers and relative paths.
        if (url.StartsWith(_appPluginsPrefix, StringComparison.OrdinalIgnoreCase) is false)
        {
            return url;
        }

        // The author already manages this URL's query — leave it alone.
        if (url.Contains('?'))
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
