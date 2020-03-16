using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class RuntimeSettings : IRuntimeSettings
    {
        private readonly IConfiguration _configuration;
        public RuntimeSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int? MaxQueryStringLength => _configuration.GetValue<int?>("Umbraco:CMS:Runtime:MaxRequestLength");
        public int? MaxRequestLength => _configuration.GetValue<int?>("Umbraco:CMS:Runtime:MaxRequestLength");
    }
}
