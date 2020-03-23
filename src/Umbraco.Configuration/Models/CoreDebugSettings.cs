using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class CoreDebugSettings : ICoreDebugSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "Core:Debug:";
        private readonly IConfiguration _configuration;

        public CoreDebugSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool LogUncompletedScopes =>
            _configuration.GetValue(Prefix+"LogUncompletedScopes", false);

        public bool DumpOnTimeoutThreadAbort =>
            _configuration.GetValue(Prefix+"DumpOnTimeoutThreadAbort", false);
    }
}
