using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class KeepAliveSettings : IKeepAliveSettings
    {
        private readonly IConfiguration _configuration;

        public KeepAliveSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool DisableKeepAliveTask =>
            _configuration.GetValue("Umbraco:CMS:KeepAlive:DisableKeepAliveTask", false);

        public string KeepAlivePingUrl => _configuration.GetValue("Umbraco:CMS:KeepAlive:KeepAlivePingUrl",
            "{umbracoApplicationUrl}/api/keepalive/ping");
    }
}
