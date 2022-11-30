using Microsoft.Extensions.Options;
using Smidge;
using Smidge.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.RuntimeMinification;

/// <summary>
///     Constructs a cache buster string with sensible defaults.
/// </summary>
/// <remarks>
///     <para>
///         Had planned on handling all of this in SmidgeRuntimeMinifier, but that only handles some urls.
///     </para>
///     <para>
///         A lot of the work is delegated e.g. to <see cref="SmidgeHelper.GenerateJsUrlsAsync(string, bool)" />
///         which doesn't care about the cache buster string in our classes only the value returned by the resolved
///         ICacheBuster.
///     </para>
///     <para>
///         Then I thought fine I'll just use a IConfigureOptions to tweak upstream <see cref="ConfigCacheBuster" />, but
///         that only cares about the <see cref="SmidgeConfig" />
///         class we instantiate and pass through in
///         <see cref="Umbraco.Extensions.UmbracoBuilderExtensions.AddRuntimeMinifier" />
///     </para>
///     <para>
///         So here we are, create our own to ensure we cache bust in a reasonable fashion.
///     </para>
///     <br /><br />
///     <para>
///         Note that this class makes some other bits of code pretty redundant e.g.
///         <see cref="UrlHelperExtensions.GetUrlWithCacheBust" /> will
///         concatenate version with CacheBuster value and hash again, but there's no real harm so can think about that
///         later.
///     </para>
/// </remarks>
internal class UmbracoSmidgeConfigCacheBuster : ICacheBuster
{
    private readonly IEntryAssemblyMetadata _entryAssemblyMetadata;
    private readonly IOptions<RuntimeMinificationSettings> _runtimeMinificationSettings;
    private readonly IUmbracoVersion _umbracoVersion;

    private string? _cacheBusterValue;

    public UmbracoSmidgeConfigCacheBuster(
        IOptions<RuntimeMinificationSettings> runtimeMinificationSettings,
        IUmbracoVersion umbracoVersion,
        IEntryAssemblyMetadata entryAssemblyMetadata)
    {
        _runtimeMinificationSettings = runtimeMinificationSettings ??
                                       throw new ArgumentNullException(nameof(runtimeMinificationSettings));
        _umbracoVersion = umbracoVersion ?? throw new ArgumentNullException(nameof(umbracoVersion));
        _entryAssemblyMetadata =
            entryAssemblyMetadata ?? throw new ArgumentNullException(nameof(entryAssemblyMetadata));
    }

    private string CacheBusterValue
    {
        get
        {
            if (_cacheBusterValue != null)
            {
                return _cacheBusterValue;
            }

            // Assembly Name adds a bit of uniqueness across sites when version missing from config.
            // Adds a bit of security through obscurity that was asked for in standup.
            var prefix = _runtimeMinificationSettings.Value.Version ?? _entryAssemblyMetadata.Name;
            var umbracoVersion = _umbracoVersion.SemanticVersion.ToString();
            var downstreamVersion = _entryAssemblyMetadata.InformationalVersion;

            _cacheBusterValue = $"{prefix}_{umbracoVersion}_{downstreamVersion}".GenerateHash();

            return _cacheBusterValue;
        }
    }

    public string GetValue() => CacheBusterValue;
}
