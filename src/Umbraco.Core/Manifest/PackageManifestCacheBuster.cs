using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Computes a package's cache-bust value and appends it to its <c>/App_Plugins</c> importmap asset URLs. The same
///     value is surfaced on the manifest response so extension asset URLs can be stamped client-side; the
///     <c>%CACHE_BUSTER%</c> token is resolved separately, server-side.
/// </summary>
public static class PackageManifestCacheBuster
{
    private const int ShortHashLength = 7;

    // The /App_Plugins root with a trailing slash, so the StartsWith check honours a path-segment boundary
    // (and never matches e.g. "/App_PluginsFoo/...").
    private static readonly string _appPluginsPrefix = Constants.SystemDirectories.AppPlugins.EnsureEndsWith('/');

    /// <summary>
    ///     Computes a package's cache-bust value as <c>&lt;version&gt;-&lt;hash&gt;</c>, where <c>hash</c> is the first
    ///     seven characters of the host <paramref name="cacheBuster"/>'s hash (a git-style short hash). Falls back to the
    ///     version alone when no host cache-buster is set, to the short hash alone when the package has no version, and
    ///     to <c>null</c> when there is nothing to bust.
    /// </summary>
    public static string? ComputeCacheBuster(string? version, string? cacheBuster)
    {
        var hasVersion = string.IsNullOrWhiteSpace(version) is false;
        var shortHash = string.IsNullOrWhiteSpace(cacheBuster)
            ? null
            : cacheBuster.GenerateHash()[..ShortHashLength];

        return (hasVersion, shortHash) switch
        {
            (true, not null) => $"{version}-{shortHash}",
            (true, null) => version,
            (false, not null) => shortHash,
            _ => null,
        };
    }

    /// <summary>
    ///     Appends <c>?umb__rnd=&lt;cacheBuster&gt;</c> to a clean <c>/App_Plugins</c> URL. URLs outside
    ///     <c>/App_Plugins</c>, URLs that already carry a query string, and a <c>null</c>/empty
    ///     <paramref name="cacheBuster"/> are returned unchanged.
    /// </summary>
    public static string ApplyCacheBust(string url, string? cacheBuster)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(cacheBuster))
        {
            return url;
        }

        if (url.StartsWith(_appPluginsPrefix, StringComparison.OrdinalIgnoreCase) is false || url.Contains('?'))
        {
            return url;
        }

        var query = $"umb__rnd={Uri.EscapeDataString(cacheBuster)}";
        var fragmentIndex = url.IndexOf('#');
        return fragmentIndex < 0
            ? $"{url}?{query}"
            : $"{url[..fragmentIndex]}?{query}{url[fragmentIndex..]}";
    }
}
