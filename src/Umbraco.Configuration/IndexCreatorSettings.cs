using System.Configuration;
using Umbraco.Abstractions;

namespace Umbraco.Configuration
{
    public class IndexCreatorSettings : IIndexCreatorSettings
    {
        public IndexCreatorSettings()
        {
           LuceneDirectoryFactory = ConfigurationManager.AppSettings["Umbraco.Examine.LuceneDirectoryFactory"]; 
        }

        public string LuceneDirectoryFactory { get; }
    }
}
