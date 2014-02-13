using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingCropSizeElement : ConfigurationElement, IImagingCropSize
    {
        [ConfigurationProperty("alias", IsRequired = true)]
        public string Alias
        {
            get { return (string)base["alias"]; }
        }

        [ConfigurationProperty("width", IsRequired = true)]
        public int Width
        {
            get { return (int)base["width"]; }
        }

        [ConfigurationProperty("height", IsRequired = true)]
        public int Height
        {
            get { return (int)base["height"]; }
        }
    }
}