using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Manifest;

/// <summary>
///     Computes and applies per-package cache-busting tokens for package manifest assets.
/// </summary>
public static partial class PackageManifestCacheBuster
{
    private const string QueryParameterName = "umb__rnd";

    // The /App_Plugins root with a trailing slash, so the StartsWith check honours a path-segment boundary
    // (and never matches e.g. "/App_PluginsFoo/...").
    private static readonly string _appPluginsPrefix = Constants.SystemDirectories.AppPlugins.EnsureEndsWith('/');

    // Auto-stamping only targets JavaScript entrypoints (.js / .mjs) — the non-bundler-hashed files that actually
    // need a version-driven bust. Matching by extension keeps the algorithm type-agnostic (it never needs to know
    // which manifest keys hold URLs) while avoiding rewriting a string that merely looks like an /App_Plugins path
    // but isn't a fetchable script (a route, an icon path, an alias, etc.).
    [GeneratedRegex(@"\.m?js$", RegexOptions.IgnoreCase)]
    private static partial Regex JavaScriptExtensionRegex();

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
    ///     path (importmap and extensions) derives them identically. The hash is always the package's
    ///     <see cref="PackageManifest.Version"/> hash (falling back to <paramref name="globalHash"/> only when the
    ///     package has no version) — an explicit <c>%CACHE_BUSTER%</c> token is the author's opt-in and always resolves
    ///     to it. <see cref="PackageManifest.AllowCacheBusting"/> governs only whether clean URLs are
    ///     <em>automatically</em> stamped; it does not change the hash.
    /// </summary>
    public static (string Hash, bool AutoStamp) ResolvePackageCacheBust(PackageManifest manifest, string globalHash)
        => (ResolvePackageCacheBustHash(manifest.Version, globalHash), manifest.AllowCacheBusting);

    /// <summary>
    ///     Applies cache-busting to a single manifest URL using <paramref name="hash"/>.
    ///     <para>
    ///     An explicit <c>%CACHE_BUSTER%</c> token is always resolved to <paramref name="hash"/> wherever it appears
    ///     (path or query, any host) — that is the author's deliberate opt-in. When <paramref name="autoStamp"/> is
    ///     <c>true</c>, a clean <c>/App_Plugins</c>-rooted JavaScript path (<c>.js</c>/<c>.mjs</c>) additionally gets
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
        if (JavaScriptExtensionRegex().IsMatch(path) is false)
        {
            return url;
        }

        return fragmentIndex < 0
            ? $"{url}?{QueryParameterName}={hash}"
            : $"{path}?{QueryParameterName}={hash}{url[fragmentIndex..]}";
    }
}
