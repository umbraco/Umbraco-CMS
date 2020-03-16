using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class CoreDebugSettings : ICoreDebugSettings
    {
        private readonly IConfiguration _configuration;
        public CoreDebugSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool LogUncompletedScopes => _configuration.GetValue<bool?>("Umbraco:CMS:Core:Debug:LogUncompletedScopes") ?? false;
        public bool DumpOnTimeoutThreadAbort => _configuration.GetValue<bool?>("Umbraco:CMS:Core:Debug:DumpOnTimeoutThreadAbort") ?? false;
    }
}
