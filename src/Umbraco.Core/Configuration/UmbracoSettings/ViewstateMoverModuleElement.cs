using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ViewstateMoverModuleElement : ConfigurationElement
    {
        [ConfigurationProperty("enable")]
        internal bool Enable
        {
            get { return (bool)base["enable"]; }
        }
    }
}