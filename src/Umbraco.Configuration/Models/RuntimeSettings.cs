using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class RuntimeSettings : IRuntimeSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "Runtime:";
        private readonly IConfiguration _configuration;
        public RuntimeSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int? MaxQueryStringLength => _configuration.GetValue<int?>(Prefix+"MaxRequestLength");
        public int? MaxRequestLength => _configuration.GetValue<int?>(Prefix+"MaxRequestLength");
    }
}
