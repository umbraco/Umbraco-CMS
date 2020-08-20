using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    public class KeepAliveSettings : IKeepAliveSettings
    {
        public bool DisableKeepAliveTask { get; set; } = false;

        public string KeepAlivePingUrl { get; set; } = "{umbracoApplicationUrl}/api/keepalive/ping";
    }
}
