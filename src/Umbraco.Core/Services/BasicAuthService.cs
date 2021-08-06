using System.Net;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Core.Services
{
    public class BasicAuthService : IBasicAuthService
    {
        private BasicAuthSettings _basicAuthSettings;

        public BasicAuthService(IOptionsMonitor<BasicAuthSettings> optionsMonitor)
        {
            _basicAuthSettings = optionsMonitor.CurrentValue;

            optionsMonitor.OnChange(basicAuthSettings => _basicAuthSettings = basicAuthSettings);
        }

        public bool IsBasicAuthEnabled() => _basicAuthSettings.Enabled;

        public bool IsIpAllowListed(IPAddress clientIpAddress)
        {
            foreach (var allowedIpString in _basicAuthSettings.AllowedIPs)
            {
                if(IPAddress.TryParse(allowedIpString, out var allowedIp) && clientIpAddress.Equals(allowedIp))
                {
                    return true;
                };
            }

            return false;
        }
    }
}
