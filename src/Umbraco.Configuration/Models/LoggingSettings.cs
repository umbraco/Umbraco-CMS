using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class LoggingSettings : ILoggingSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "Logging:";
        private readonly IConfiguration _configuration;

        public LoggingSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int MaxLogAge => _configuration.GetValue(Prefix + "MaxLogAge", -1);
    }
}
