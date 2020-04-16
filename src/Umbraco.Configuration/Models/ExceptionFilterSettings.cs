using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class ExceptionFilterSettings : IExceptionFilterSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "ExceptionFilter:";
        private readonly IConfiguration _configuration;

        public ExceptionFilterSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Disabled => _configuration.GetValue(Prefix+"Disabled", false);
    }
}
