using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ViewstateMoverModuleElement : ConfigurationElement, IViewStateMoverModuleSection
    {
        [ConfigurationProperty("enable", DefaultValue = false)]
        public bool Enable
        {
            get { return (bool)base["enable"]; }
        }
    }
}