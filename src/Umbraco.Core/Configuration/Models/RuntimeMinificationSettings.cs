using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

[UmbracoOptions(Constants.Configuration.ConfigRuntimeMinification)]
public class RuntimeMinificationSettings
{
    internal const bool StaticUseInMemoryCache = false;
    internal const string StaticCacheBuster = "Version";
    internal const string? StaticVersion = null;

    /// <summary>
    ///     Use in memory cache
    /// </summary>
    [DefaultValue(StaticUseInMemoryCache)]
    public bool UseInMemoryCache { get; set; } = StaticUseInMemoryCache;

    /// <summary>
    ///     The cache buster type to use
    /// </summary>
    [DefaultValue(StaticCacheBuster)]
    public RuntimeMinificationCacheBuster CacheBuster { get; set; } =
        Enum<RuntimeMinificationCacheBuster>.Parse(StaticCacheBuster);

    /// <summary>
    ///     The unique version string used if CacheBuster is 'Version'.
    /// </summary>
    [DefaultValue(StaticVersion)]
    public string? Version { get; set; } = StaticVersion;
}
