using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class AppCodeFileExtensionsElement : ConfigurationElement
    {
        [ConfigurationCollection(typeof(AppCodeFileExtensionsCollection), AddItemName = "ext")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AppCodeFileExtensionsCollection AppCodeFileExtensionsCollection
        {
            get { return (AppCodeFileExtensionsCollection)base[""]; }
        }
    }
}