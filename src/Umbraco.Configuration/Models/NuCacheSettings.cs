using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class NuCacheSettings : INuCacheSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "NuCache:";
        private readonly IConfiguration _configuration;

        public NuCacheSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string BTreeBlockSize => _configuration.GetValue<string>(Prefix+"BTreeBlockSize");
    }
}
