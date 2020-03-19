using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class ActiveDirectorySettings : IActiveDirectorySettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "ActiveDirectory:";
        private readonly IConfiguration _configuration;

        public ActiveDirectorySettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string ActiveDirectoryDomain => _configuration.GetValue<string>(Prefix+"Domain");
    }
}
