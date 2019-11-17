using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class KeepAliveElement : ConfigurationElement, IKeepAliveSection
    {
        [ConfigurationProperty("enableKeepAliveTask", DefaultValue = "true")]
        public bool EnableKeepAliveTask => (bool)base["enableKeepAliveTask"];

        [ConfigurationProperty("keepAlivePingUrl", DefaultValue = "{umbracoApplicationUrl}/api/keepalive/ping")]
        public string KeepAlivePingUrl => (string)base["keepAlivePingUrl"];
    }
}
