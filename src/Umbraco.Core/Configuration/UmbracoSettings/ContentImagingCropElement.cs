using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingCropElement : ConfigurationElement, IImagingCrop
    {
        [ConfigurationProperty("mediaTypeAlias", IsRequired = true)]
        public string MediaTypeAlias
        {
            get { return (string)base["mediaTypeAlias"]; }
        }

        [ConfigurationProperty("focalPointProperty", IsRequired = true)]
        public string FocalPointProperty
        {
            get { return (string)base["focalPointProperty"]; }
        }

        [ConfigurationProperty("fileProperty", IsRequired = true)]
        public string FileProperty
        {
            get { return (string)base["fileProperty"]; }
        }

        [ConfigurationCollection(typeof(ContentImagingCropSizeCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ContentImagingCropSizeCollection CropSizeCollection
        {
            get { return (ContentImagingCropSizeCollection)base[""]; }
            //set { base[""] = value; }
        }

        IEnumerable<IImagingCropSize> IImagingCrop.CropSizes
        {
            get { return CropSizeCollection; }            
        }
    }
}