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
                       GetDefaultImageFileTypes());
            }
        }

        internal static string[] GetDefaultImageFileTypes()
        {
            return new[] {"jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif"};
        }

        [ConfigurationProperty("allowedAttributes")]
        internal CommaDelimitedConfigurationElement ImageTagAllowedAttributes
        {
            get
            {
                return new OptionalCommaDelimitedConfigurationElement(
                       (CommaDelimitedConfigurationElement)this["allowedAttributes"],
                        //set the default
                       new[] { "src", "alt", "border", "class", "style", "align", "id", "name", "onclick", "usemap" });
            }
        }

        private ImagingAutoFillPropertiesCollection _defaultImageAutoFill;

        [ConfigurationCollection(typeof(ImagingAutoFillPropertiesCollection), AddItemName = "uploadField")]
        [ConfigurationProperty("autoFillImageProperties", IsDefaultCollection = true)]
        internal ImagingAutoFillPropertiesCollection ImageAutoFillProperties
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
                    _defaultImageAutoFill = new ImagingAutoFillPropertiesCollection
                        {
                            new ImagingAutoFillUploadFieldElement
                                {
                                    Alias = "umbracoFile"
                                }
                        };
                    return _defaultImageAutoFill;
                }
                
                return (ImagingAutoFillPropertiesCollection) base["autoFillImageProperties"];
            }
        }

        internal static ImagingAutoFillPropertiesCollection GetDefaultImageAutoFillProperties()
        {
            return new ImagingAutoFillPropertiesCollection
                        {
                            new ImagingAutoFillUploadFieldElement
                                {
                                    Alias = "umbracoFile"
                                }
                        };
        }

    }
}