using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class NodeEditsConfigElement : UmbracoConfigurationElement, INodeEditsSection
    {
        [ConfigurationProperty("enable", DefaultValue = false)]
        public bool EnableNodeEdits
        {
            get { return (bool)this["enable"]; }
        }
    }
}
