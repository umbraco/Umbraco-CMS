using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RazorElement : ConfigurationElement
    {
        [ConfigurationCollection(typeof (NotDynamicXmlDocumentElementCollection), AddItemName = "element")]
        [ConfigurationProperty("notDynamicXmlDocumentElements", IsDefaultCollection = true)]
        public NotDynamicXmlDocumentElementCollection NotDynamicXmlDocumentElements
        {
            get { return (NotDynamicXmlDocumentElementCollection) base["notDynamicXmlDocumentElements"]; }
        }

        [ConfigurationCollection(typeof (RazorStaticMappingCollection), AddItemName = "mapping")]
        [ConfigurationProperty("dataTypeModelStaticMappings", IsDefaultCollection = true)]
        public RazorStaticMappingCollection DataTypeModelStaticMappings
        {
            get { return (RazorStaticMappingCollection) base["dataTypeModelStaticMappings"]; }
        }
    }
}