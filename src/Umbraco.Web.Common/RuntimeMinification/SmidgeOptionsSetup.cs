using Microsoft.Extensions.Options;
using Smidge.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.RuntimeMinification;

public class SmidgeOptionsSetup : IConfigureOptions<SmidgeOptions>
{
    private readonly IOptions<RuntimeMinificationSettings> _runtimeMinificatinoSettings;

    public SmidgeOptionsSetup(IOptions<RuntimeMinificationSettings> runtimeMinificatinoSettings)
        => _runtimeMinificatinoSettings = runtimeMinificatinoSettings;

    /// <summary>
    ///     Configures Smidge to use in-memory caching if configured that way or if certain cache busters are used
    /// </summary>
    /// <param name="options"></param>
    public void Configure(SmidgeOptions options)
        => options.CacheOptions.UseInMemoryCache = _runtimeMinificatinoSettings.Value.UseInMemoryCache ||
                                                   _runtimeMinificatinoSettings.Value.CacheBuster ==
                                                   RuntimeMinificationCacheBuster.Timestamp;
}
