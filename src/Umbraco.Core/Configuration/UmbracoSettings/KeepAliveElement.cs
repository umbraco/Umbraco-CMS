using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class KeepAliveElement : ConfigurationElement, IKeepAliveSection
    {
        [ConfigurationProperty("disableKeepAliveTask", DefaultValue = "false")]
        public bool DisableKeepAliveTask => (bool)base["disableKeepAliveTask"];

        [ConfigurationProperty("keepAlivePingUrl", DefaultValue = "{umbracoApplicationUrl}/api/keepalive/ping")]
        public string KeepAlivePingUrl => (string)base["keepAlivePingUrl"];
    }
}
