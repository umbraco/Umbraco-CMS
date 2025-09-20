using Microsoft.Extensions.Options;
using Smidge.Cache;
using Smidge.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.RuntimeMinification;

public class SmidgeOptionsSetup : IConfigureOptions<SmidgeOptions>
{
    private readonly IOptions<RuntimeMinificationSettings> _runtimeMinificationSettings;

    public SmidgeOptionsSetup(IOptions<RuntimeMinificationSettings> runtimeMinificatinoSettings)
        => _runtimeMinificationSettings = runtimeMinificatinoSettings;

    /// <summary>
    /// Configures Smidge to use in-memory caching if configured that way or if certain cache busters are used.
    /// Also sets the cache buster type such that public facing bundles will use the configured method.
    /// </summary>
    /// <param name="options">Instance of <see cref="SmidgeOptions"></see> to configure.</param>
    public void Configure(SmidgeOptions options)
    {
        options.CacheOptions.UseInMemoryCache = _runtimeMinificationSettings.Value.UseInMemoryCache ||
                                                   _runtimeMinificationSettings.Value.CacheBuster ==
                                                   RuntimeMinificationCacheBuster.Timestamp;

        Type cacheBusterType = _runtimeMinificationSettings.Value.CacheBuster switch
        {
            RuntimeMinificationCacheBuster.AppDomain => typeof(AppDomainLifetimeCacheBuster),
            RuntimeMinificationCacheBuster.Version => typeof(UmbracoSmidgeConfigCacheBuster),
            RuntimeMinificationCacheBuster.Timestamp => typeof(TimestampCacheBuster),
            _ => throw new ArgumentOutOfRangeException("CacheBuster", $"{_runtimeMinificationSettings.Value.CacheBuster} is not a valid value for RuntimeMinificationCacheBuster."),
        };

        options.DefaultBundleOptions.DebugOptions.SetCacheBusterType(cacheBusterType);
        options.DefaultBundleOptions.ProductionOptions.SetCacheBusterType(cacheBusterType);
    }
}
