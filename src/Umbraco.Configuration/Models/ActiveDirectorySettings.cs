using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class ActiveDirectorySettings : IActiveDirectorySettings
    {
        private readonly IConfiguration _configuration;

        public ActiveDirectorySettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ActiveDirectoryDomain => _configuration.GetValue<string>("Umbraco:CMS:ActiveDirectory:Domain");
    }
}
