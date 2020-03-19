using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Implementations
{
    internal class KeepAliveSettings : ConfigurationManagerConfigBase, IKeepAliveSettings
    {
        public bool DisableKeepAliveTask  => UmbracoSettingsSection.KeepAlive.DisableKeepAliveTask;
        public string KeepAlivePingUrl  => UmbracoSettingsSection.KeepAlive.KeepAlivePingUrl;
    }
}
