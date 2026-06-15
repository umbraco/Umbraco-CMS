using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Computes and applies per-package cache-busting tokens for package manifest assets.
/// </summary>
public static class PackageManifestCacheBuster
{
    private const string QueryParameterName = "umb__rnd";

    // Auto-stamping only targets JavaScript entrypoints — the non-bundler-hashed files that actually need a
    // version-driven bust. Restricting by file extension keeps the algorithm type-agnostic (it never needs to know
    // which manifest keys hold URLs) while avoiding rewriting a string that merely looks like an /App_Plugins path
    // but isn't a fetchable script (a route, an icon path, an alias, etc.).
    private const string JavaScriptExtension = ".js";

    // The /App_Plugins root with a trailing slash, so the StartsWith check honours a path-segment boundary
    // (and never matches e.g. "/App_PluginsFoo/...").
    private static readonly string _appPluginsPrefix = Constants.SystemDirectories.AppPlugins.EnsureEndsWith('/');

    /// <summary>
    ///     Returns the cache-bust hash for a package: a hash of its <paramref name="packageVersion"/> when present,
    ///     otherwise the supplied global fallback hash.
    /// </summary>
    public static string ResolvePackageCacheBustHash(string? packageVersion, string fallbackHash)
        => string.IsNullOrWhiteSpace(packageVersion)
            ? fallbackHash
            : packageVersion.GenerateHash();

    /// <summary>
    ///     Resolves the cache-bust hash and auto-stamp flag for a package in one place, so every manifest-processing
    ///     path (importmap and extensions) derives them identically. When the package opts into cache-busting the hash
    ///     comes from its <see cref="PackageManifest.Version"/> (falling back to <paramref name="globalHash"/>) and
    ///     auto-stamping is on; when it opts out the hash is <paramref name="globalHash"/> and auto-stamping is off (an
    ///     explicit <c>%CACHE_BUSTER%</c> token still resolves, to the global hash).
    /// </summary>
    public static (string Hash, bool AutoStamp) ResolvePackageCacheBust(PackageManifest manifest, string globalHash)
    {
        var autoStamp = manifest.AllowCacheBusting;
        var hash = autoStamp
            ? ResolvePackageCacheBustHash(manifest.Version, globalHash)
            : globalHash;

        return (hash, autoStamp);
    }

    /// <summary>
    ///     Applies cache-busting to a single manifest URL using <paramref name="hash"/>.
    ///     <para>
    ///     An explicit <c>%CACHE_BUSTER%</c> token is always resolved to <paramref name="hash"/> wherever it appears
    ///     (path or query, any host) — that is the author's deliberate opt-in. When <paramref name="autoStamp"/> is
    ///     <c>true</c>, a clean <c>/App_Plugins</c>-rooted <c>.js</c> path additionally gets
    ///     <c>?umb__rnd=&lt;hash&gt;</c> appended. Everything else — the backoffice core (<c>/umbraco/backoffice/...</c>),
    ///     CDNs, protocol-relative URLs, bare module specifiers, relative paths, non-JavaScript assets, and URLs that
    ///     already carry a query string — is returned unchanged.
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

        var fragmentIndex = url.IndexOf('#');
        var path = fragmentIndex < 0 ? url : url[..fragmentIndex];

        // Only JavaScript entrypoints are auto-stamped; other asset shapes are left untouched.
        if (path.EndsWith(JavaScriptExtension, StringComparison.OrdinalIgnoreCase) is false)
        {
            return url;
        }

        return fragmentIndex < 0
            ? $"{url}?{QueryParameterName}={hash}"
            : $"{path}?{QueryParameterName}={hash}{url[fragmentIndex..]}";
    }
}
