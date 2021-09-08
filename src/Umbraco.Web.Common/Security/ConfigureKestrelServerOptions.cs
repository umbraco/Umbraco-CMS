using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Web.Common.Security
{
    public class ConfigureKestrelServerOptions : IConfigureOptions<KestrelServerOptions>
    {
        private readonly IOptions<RuntimeSettings> _runtimeSettings;

        public ConfigureKestrelServerOptions(IOptions<RuntimeSettings> runtimeSettings) => _runtimeSettings = runtimeSettings;
        public void Configure(KestrelServerOptions options)
        {
            // convert from KB to bytes
            options.Limits.MaxRequestBodySize = _runtimeSettings.Value.MaxRequestLength * 1024;
        }
    }
}
