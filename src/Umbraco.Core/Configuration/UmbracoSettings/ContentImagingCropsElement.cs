using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentImagingCropsElement : ConfigurationElement, IImagingCrops
    {
        [ConfigurationProperty("saveFiles", DefaultValue = false)]
        public bool SaveFiles
        {
            get { return (bool)base["saveFiles"]; }
        }

        [ConfigurationCollection(typeof(ContentImagingCropCollection), AddItemName = "crop")]
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ContentImagingCropCollection CropCollection
        {
            get { return (ContentImagingCropCollection)base[""]; }
            //set { base[""] = value; }
        }
        
        IEnumerable<IImagingCrop> IImagingCrops.Crops
        {
            get { return CropCollection; }
        }
    }
}