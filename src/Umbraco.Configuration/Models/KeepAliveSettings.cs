using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class KeepAliveSettings : IKeepAliveSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "KeepAlive:";
        private readonly IConfiguration _configuration;

        public KeepAliveSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool DisableKeepAliveTask =>
            _configuration.GetValue(Prefix + "DisableKeepAliveTask", false);

        public string KeepAlivePingUrl => _configuration.GetValue(Prefix + "KeepAlivePingUrl",
            "{umbracoApplicationUrl}/api/keepalive/ping");
    }
}
