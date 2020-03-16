using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class ExceptionFilterSettings : IExceptionFilterSettings
    {
        private readonly IConfiguration _configuration;

        public ExceptionFilterSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Disabled => _configuration.GetValue("Umbraco:CMS:ExceptionFilter:Disabled", false);
    }
}
