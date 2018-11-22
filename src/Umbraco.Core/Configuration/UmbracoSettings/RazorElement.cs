using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RazorElement : ConfigurationElement
    {
        private NotDynamicXmlDocumentElementCollection _defaultCollection;

        [ConfigurationCollection(typeof (NotDynamicXmlDocumentElementCollection), AddItemName = "element")]
        [ConfigurationProperty("notDynamicXmlDocumentElements", IsDefaultCollection = true)]
        internal NotDynamicXmlDocumentElementCollection NotDynamicXmlDocumentElements
        {
            get
            {
                if (_defaultCollection != null)
                {
                    return _defaultCollection;
                }

                //here we need to check if this element is defined, if it is not then we'll setup the defaults
                var prop = Properties["notDynamicXmlDocumentElements"];
                var autoFill = this[prop] as ConfigurationElementCollection;
                if (autoFill != null && autoFill.ElementInformation.IsPresent == false)
                {
                    _defaultCollection = GetDefaultNotDynamicXmlDocuments();

                    //must return the collection directly
                    return _defaultCollection;
                }

                return (NotDynamicXmlDocumentElementCollection)base["notDynamicXmlDocumentElements"];
            }
        }

        internal static NotDynamicXmlDocumentElementCollection GetDefaultNotDynamicXmlDocuments()
        {
            return new NotDynamicXmlDocumentElementCollection
                        {
                            new NotDynamicXmlDocumentElement {RawValue = "p"},
                            new NotDynamicXmlDocumentElement {RawValue = "div"},
                            new NotDynamicXmlDocumentElement {RawValue = "ul"},
                            new NotDynamicXmlDocumentElement {RawValue = "span"}
                        };
        }

        [ConfigurationCollection(typeof (RazorStaticMappingCollection), AddItemName = "mapping")]
        [ConfigurationProperty("dataTypeModelStaticMappings", IsDefaultCollection = true)]
        internal RazorStaticMappingCollection DataTypeModelStaticMappings
        {
            get { return (RazorStaticMappingCollection) base["dataTypeModelStaticMappings"]; }
        }

    }
}