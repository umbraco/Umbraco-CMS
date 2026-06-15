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
    ///     developing), otherwise a stable hash of the Umbraco semantic version.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment, used to determine whether the application runs in debug mode.</param>
    /// <param name="umbracoVersion">The Umbraco version, hashed as the cache-bust value outside debug mode.</param>
    public static string Generate(IHostingEnvironment hostingEnvironment, IUmbracoVersion umbracoVersion)
        => hostingEnvironment.IsDebugMode
            ? DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).GenerateHash()
            : umbracoVersion.SemanticVersion.ToSemanticString().GenerateHash();
}
