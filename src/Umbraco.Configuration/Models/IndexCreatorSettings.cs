using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Models
{
    internal class IndexCreatorSettings : IIndexCreatorSettings
    {
        private readonly IConfiguration _configuration;

        public IndexCreatorSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string LuceneDirectoryFactory =>
            _configuration.GetValue<string>("Umbraco:CMS:Examine:LuceneDirectoryFactory");
    }
}
