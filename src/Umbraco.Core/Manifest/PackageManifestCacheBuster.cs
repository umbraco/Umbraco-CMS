using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Computes and applies per-package cache-busting tokens for package manifest assets.
/// </summary>
public static class PackageManifestCacheBuster
{
    private const string QueryParameterName = "umb__rnd";

    /// <summary>
    ///     Returns the cache-bust hash for a package: a hash of its <paramref name="packageVersion"/> when present,
    ///     otherwise the supplied global fallback hash.
    /// </summary>
    public static string ResolvePackageCacheBustHash(string? packageVersion, string fallbackHash)
        => string.IsNullOrWhiteSpace(packageVersion)
            ? fallbackHash
            : packageVersion.GenerateHash();

    /// <summary>
    ///     Computes Umbraco's global cache-bust hash (mirrors <c>UrlHelperExtensions.GetCacheBustHash</c>): a
    ///     restart-varying hash in debug mode, otherwise a hash of the Umbraco semantic version.
    /// </summary>
    public static string GetGlobalCacheBustHash(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion)
        => hostingEnvironment.IsDebugMode
            ? DateTime.Now.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture).GenerateHash()
            : umbracoVersion.SemanticVersion.ToSemanticString().GenerateHash();

    /// <summary>
    ///     Appends <c>?umb__rnd=&lt;hash&gt;</c> to a URL when, and only when, it is a clean <c>/App_Plugins</c>-rooted
    ///     path. URLs that carry the <c>%CACHE_BUSTER%</c> token, already have a query string, or point anywhere other
    ///     than <c>/App_Plugins</c> (backoffice core, CDN, bare module specifiers, relative paths) are returned unchanged.
    /// </summary>
    public static string ApplyCacheBust(string url, string hash)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        // %CACHE_BUSTER% is the explicit opt-in token, resolved elsewhere to the global hash — never auto-stamp it.
        if (url.Contains(Constants.Web.CacheBusterToken, StringComparison.Ordinal))
        {
            return url;
        }

        // Only ever touch the package's own /App_Plugins assets. This excludes the backoffice core
        // (/umbraco/backoffice/...), CDNs, protocol-relative URLs, bare specifiers and relative paths.
        if (url.StartsWith(Constants.SystemDirectories.AppPlugins, StringComparison.OrdinalIgnoreCase) is false)
        {
            return url;
        }

        // The author already manages this URL's query — leave it alone.
        if (url.Contains('?', StringComparison.Ordinal))
        {
            return url;
        }

        var fragmentIndex = url.IndexOf('#', StringComparison.Ordinal);
        return fragmentIndex < 0
            ? $"{url}?{QueryParameterName}={hash}"
            : $"{url[..fragmentIndex]}?{QueryParameterName}={hash}{url[fragmentIndex..]}";
    }
}
