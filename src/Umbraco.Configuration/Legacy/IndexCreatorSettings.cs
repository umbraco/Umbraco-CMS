using System.Configuration;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration.Legacy
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
