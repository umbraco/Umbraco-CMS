namespace Umbraco.Cms.Core.Plugin;

public interface IPluginConfigurationService
{
    Task<IEnumerable<PluginConfiguration>> GetPluginConfigurationsAsync();
}
