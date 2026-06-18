using System.Globalization;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Web;

/// <summary>
///     Generates the cache-bust hash used to version Umbraco's web assets.
/// </summary>
public static class CacheBustHashGenerator
{
    /// <summary>
    ///     Computes the cache-bust hash: a restart-varying hash in debug mode (so assets are always re-fetched while
    ///     developing), otherwise a stable hash of the Umbraco semantic version. When a non-empty <paramref name="seed"/>
    ///     is supplied (a host-controlled value, e.g. a deployment id from configuration), it is mixed in so that
    ///     changing it forces a fresh hash — and therefore busts every asset derived from it — regardless of the
    ///     Umbraco version. An empty seed leaves the hash unchanged.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment, used to determine whether the application runs in debug mode.</param>
    /// <param name="umbracoVersion">The Umbraco version, hashed as the cache-bust value outside debug mode.</param>
    /// <param name="seed">An optional host-controlled value mixed into the hash to force a global cache-bust on demand.</param>
    public static string Generate(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion, string? seed = null)
    {
        var baseHash = hostingEnvironment.IsDebugMode
            ? DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).GenerateHash()
            : umbracoVersion.SemanticVersion.ToSemanticString().GenerateHash();

        return string.IsNullOrWhiteSpace(seed)
            ? baseHash
            : $"{baseHash}|{seed}".GenerateHash();
    }
}
