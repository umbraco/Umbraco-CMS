using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingElement : ConfigurationElement
    {
        [ConfigurationProperty("imageFileTypes")]
        internal CommaDelimitedConfigurationElement ImageFileTypes
        {
            get
            {
                return new OptionalCommaDelimitedConfigurationElement(
                       (CommaDelimitedConfigurationElement)this["imageFileTypes"],
                        //set the default
                       new[] { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" });
            }
        }

        [ConfigurationProperty("allowedAttributes")]
        internal CommaDelimitedConfigurationElement AllowedAttributes
        {
            get
            {
                return new OptionalCommaDelimitedConfigurationElement(
                       (CommaDelimitedConfigurationElement)this["allowedAttributes"],
                        //set the default
                       new[] { "src", "alt", "border", "class", "style", "align", "id", "name", "onclick", "usemap" });
            }
        }

        [ConfigurationCollection(typeof(ContentImagingAutoFillPropertiesCollection), AddItemName = "uploadField")]
        [ConfigurationProperty("autoFillImageProperties", IsDefaultCollection = true, IsRequired = true)]        
        public ContentImagingAutoFillPropertiesCollection ImageAutoFillProperties
        {
            get { return (ContentImagingAutoFillPropertiesCollection) base["autoFillImageProperties"]; }
        }

    }
}