using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class LoggingSettings : ILoggingSettings
    {
        private readonly IConfiguration _configuration;
        public LoggingSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public int MaxLogAge => _configuration.GetValue<int?>("Umbraco:CMS:Logging:MaxLogAge") ?? -1;
    }
}
