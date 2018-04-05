using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class BackOfficeElement : UmbracoConfigurationElement, IBackOfficeSection
    {
        [ConfigurationProperty("tours")]
        internal TourConfigElement Tours
        {
            get { return (TourConfigElement)this["tours"]; }
        }

        
        [ConfigurationProperty("nodeEdits")]
        internal NodeEditsConfigElement NodeEdits
        {
            get { return (NodeEditsConfigElement)this["nodeEdits"]; }
        }


        ITourSection IBackOfficeSection.Tours
        {
            get { return Tours; }
        }

        INodeEditsSection IBackOfficeSection.NodeEdits
        {
            get { return NodeEdits; }
        }
    }
}
