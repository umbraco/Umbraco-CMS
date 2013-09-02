using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ViewstateMoverModuleElement : ConfigurationElement
    {
        [ConfigurationProperty("enable", DefaultValue = false)]
        internal bool Enable
        {
            get { return (bool)base["enable"]; }
        }
    }
}