using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingElement : ConfigurationElement, IContentImaging
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

        private ContentImagingAutoFillPropertiesCollection _defaultImageAutoFill;

        [ConfigurationCollection(typeof(ContentImagingAutoFillPropertiesCollection), AddItemName = "uploadField")]
        [ConfigurationProperty("autoFillImageProperties", IsDefaultCollection = true)]
        internal ContentImagingAutoFillPropertiesCollection ImageAutoFillProperties
        {
            get
            {
                if (_defaultImageAutoFill != null)
                {
                    return _defaultImageAutoFill;
                }

                //here we need to check if this element is defined, if it is not then we'll setup the defaults
                var prop = Properties["autoFillImageProperties"];
                var autoFill = this[prop] as ConfigurationElement;
                if (autoFill != null && autoFill.ElementInformation.IsPresent == false)
                {
                    _defaultImageAutoFill = new ContentImagingAutoFillPropertiesCollection
                        {
                            new ContentImagingAutoFillUploadFieldElement
                                {
                                    Alias = "umbracoFile"
                                }
                        };
                    return _defaultImageAutoFill;
                }
                
                return (ContentImagingAutoFillPropertiesCollection) base["autoFillImageProperties"];
            }
        }


        IEnumerable<string> IContentImaging.ImageFileTypes
        {
            get { return ImageFileTypes; }
        }

        IEnumerable<string> IContentImaging.AllowedAttributes
        {
            get { return AllowedAttributes; }
        }

        IEnumerable<IContentImagingAutoFillUploadField> IContentImaging.ImageAutoFillProperties
        {
            get { return ImageAutoFillProperties; }
        }
    }
}