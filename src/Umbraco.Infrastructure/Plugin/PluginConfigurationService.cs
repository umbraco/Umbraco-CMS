using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Plugin;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Plugin;

internal sealed class PluginConfigurationService : IPluginConfigurationService
{
    private readonly IPluginConfigurationReader _pluginConfigurationReader;
    private readonly IAppPolicyCache _cache;

    public PluginConfigurationService(IPluginConfigurationReader pluginConfigurationReader, AppCaches appCaches)
    {
        _pluginConfigurationReader = pluginConfigurationReader;
        _cache = appCaches.RuntimeCache;
    }

    public async Task<IEnumerable<PluginConfiguration>> GetPluginConfigurationsAsync()
        => await _cache.GetCacheItemAsync(
               $"{nameof(PluginConfigurationService)}-PluginConfigurations",
               async () => await _pluginConfigurationReader.ReadPluginConfigurationsAsync(),
               TimeSpan.FromMinutes(10))
           ?? Array.Empty<PluginConfiguration>();
}
