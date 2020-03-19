using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class IndexCreatorSettings : IIndexCreatorSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "Examine:";
        private readonly IConfiguration _configuration;

        public IndexCreatorSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string LuceneDirectoryFactory =>
            _configuration.GetValue<string>(Prefix + "LuceneDirectoryFactory");
    }
}
