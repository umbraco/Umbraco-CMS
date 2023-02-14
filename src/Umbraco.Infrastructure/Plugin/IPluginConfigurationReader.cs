using Umbraco.Cms.Core.Plugin;

namespace Umbraco.Cms.Infrastructure.Plugin;

public interface IPluginConfigurationReader
{
    Task<IEnumerable<PluginConfiguration>> ReadPluginConfigurationsAsync();
}
