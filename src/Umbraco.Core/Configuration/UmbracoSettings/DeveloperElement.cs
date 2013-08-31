using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class DeveloperElement : ConfigurationElement
    {
        [ConfigurationProperty("appCodeFileExtensions")]
        internal AppCodeFileExtensionsElement AppCodeFileExtensions
        {
            get { return (AppCodeFileExtensionsElement)this["appCodeFileExtensions"]; }
        }
    }
}