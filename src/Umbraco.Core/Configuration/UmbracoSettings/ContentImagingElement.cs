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
        [ConfigurationProperty("autoFillImageProperties", IsDefaultCollection = true)]        
        public ContentImagingAutoFillPropertiesCollection ImageAutoFillProperties
        {
            get
            {
                //here we need to check if this element is defined, if it is not then we'll setup the defaults
                var prop = Properties["autoFillImageProperties"];
                var autoFill = this[prop] as ConfigurationElement;
                if (autoFill != null && autoFill.ElementInformation.IsPresent == false)
                {
                    var collection = new ContentImagingAutoFillPropertiesCollection();
                    collection.Add(new ContentImagingAutoFillUploadFieldElement
                        {
                            Alias = "umbracoFile"
                        });
                    base["autoFillImageProperties"] = collection;
                }
                
                return (ContentImagingAutoFillPropertiesCollection) base["autoFillImageProperties"];
            }
        }

    }
}