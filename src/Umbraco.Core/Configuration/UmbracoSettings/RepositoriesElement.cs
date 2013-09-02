using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RepositoriesElement : ConfigurationElement
    {
        [ConfigurationCollection(typeof(RepositoriesCollection), AddItemName = "repository")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public RepositoriesCollection Repositories
        {
            get { return (RepositoriesCollection)base[""]; }
        }
    }
}