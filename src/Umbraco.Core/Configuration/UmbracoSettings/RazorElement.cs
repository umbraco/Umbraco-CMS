using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RazorElement : ConfigurationElement
    {
        [ConfigurationCollection(typeof (NotDynamicXmlDocumentElementCollection), AddItemName = "element")]
        [ConfigurationProperty("notDynamicXmlDocumentElements", IsDefaultCollection = true)]
        public NotDynamicXmlDocumentElementCollection NotDynamicXmlDocumentElements
        {
            get
            {
                //here we need to check if this element is defined, if it is not then we'll setup the defaults
                var prop = Properties["notDynamicXmlDocumentElements"];
                var autoFill = this[prop] as ConfigurationElement;
                if (autoFill != null && autoFill.ElementInformation.IsPresent == false)
                {
                    var collection = new NotDynamicXmlDocumentElementCollection
                        {
                            new NotDynamicXmlDocumentElement {RawValue = "p"},
                            new NotDynamicXmlDocumentElement {RawValue = "div"},
                            new NotDynamicXmlDocumentElement {RawValue = "ul"},
                            new NotDynamicXmlDocumentElement {RawValue = "span"}
                        };

                    return collection;
                }

                return (NotDynamicXmlDocumentElementCollection)base["notDynamicXmlDocumentElements"];
            }
        }

        [ConfigurationCollection(typeof (RazorStaticMappingCollection), AddItemName = "mapping")]
        [ConfigurationProperty("dataTypeModelStaticMappings", IsDefaultCollection = true)]
        public RazorStaticMappingCollection DataTypeModelStaticMappings
        {
            get { return (RazorStaticMappingCollection) base["dataTypeModelStaticMappings"]; }
        }
    }
}