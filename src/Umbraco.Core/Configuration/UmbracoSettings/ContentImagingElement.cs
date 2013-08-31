using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingElement : ConfigurationElement
    {
        [ConfigurationProperty("imageFileTypes")]
        internal CommaDelimitedConfigurationElement ImageFileTypes
        {
            get { return (CommaDelimitedConfigurationElement)this["imageFileTypes"]; }
        }

        [ConfigurationProperty("allowedAttributes")]
        internal CommaDelimitedConfigurationElement AllowedAttributes
        {
            get { return (CommaDelimitedConfigurationElement)this["allowedAttributes"]; }
        }

        [ConfigurationCollection(typeof(ContentImagingAutoFillPropertiesCollection), AddItemName = "uploadField")]
        [ConfigurationProperty("autoFillImageProperties", IsDefaultCollection = true)]        
        public ContentImagingAutoFillPropertiesCollection ImageAutoFillProperties
        {
            get { return (ContentImagingAutoFillPropertiesCollection)base["autoFillImageProperties"]; }
        }

    }
}