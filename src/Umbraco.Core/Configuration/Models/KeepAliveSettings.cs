namespace Umbraco.Core.Configuration.Models
{
    public class KeepAliveSettings
    {
        public bool DisableKeepAliveTask => false;

        public string KeepAlivePingUrl => "{umbracoApplicationUrl}/api/keepalive/ping";
    }
}
