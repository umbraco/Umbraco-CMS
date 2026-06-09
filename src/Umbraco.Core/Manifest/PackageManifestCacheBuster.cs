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
    ///     Applies cache-busting to a single manifest URL using <paramref name="hash"/>.
    ///     <para>
    ///     An explicit <c>%CACHE_BUSTER%</c> token is always resolved to <paramref name="hash"/> wherever it appears
    ///     (path or query, any host) — that is the author's deliberate opt-in. When <paramref name="autoStamp"/> is
    ///     <c>true</c>, a clean <c>/App_Plugins</c>-rooted path additionally gets <c>?umb__rnd=&lt;hash&gt;</c> appended.
    ///     Everything else — the backoffice core (<c>/umbraco/backoffice/...</c>), CDNs, protocol-relative URLs, bare
    ///     module specifiers, relative paths, and URLs that already carry a query string — is returned unchanged.
    ///     </para>
    /// </summary>
    public static string ApplyCacheBust(string url, string hash, bool autoStamp)
    {
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        // The explicit %CACHE_BUSTER% opt-in: resolve it wherever the author placed it, regardless of autoStamp.
        // A URL the author already tokenised is never also auto-stamped.
        if (url.Contains(Constants.Web.CacheBusterToken, StringComparison.Ordinal))
        {
            return url.Replace(Constants.Web.CacheBusterToken, hash, StringComparison.Ordinal);
        }

        if (autoStamp is false)
        {
            return url;
        }

        // Automatic stamping only ever touches the package's own /App_Plugins assets. The trailing slash enforces a
        // path-segment boundary so we never match e.g. "/App_PluginsFoo/...". This also excludes the backoffice core
        // (/umbraco/backoffice/...), CDNs, protocol-relative URLs, bare specifiers and relative paths.
        if (url.StartsWith($"{Constants.SystemDirectories.AppPlugins}/", StringComparison.OrdinalIgnoreCase) is false)
        {
            return url;
        }

        // The author already manages this URL's query — leave it alone.
        if (url.Contains('?'))
        {
            return url;
        }

        var fragmentIndex = url.IndexOf('#');
        return fragmentIndex < 0
            ? $"{url}?{QueryParameterName}={hash}"
            : $"{url[..fragmentIndex]}?{QueryParameterName}={hash}{url[fragmentIndex..]}";
    }
}
