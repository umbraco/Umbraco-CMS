using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.PropertyEditors
{
    internal class ImageCropperPropertyEditorHelper
    {
        
        internal static Umbraco.Web.Models.ImageCropData GetCrop(string mediaTypeAlias, string cropAlias){

            return null;

            /*var _crops = GetConfigurationForType(mediaTypeAlias);
            
            if (_crops == null || _crops.Crops == null)
                return null;

            return _crops.Crops[cropAlias];*/
        }

        //this queries all crops configured
        internal static Umbraco.Web.Models.ImageCropData GetCrop(string cropAlias)
        {
            /*
            foreach (var typeCrops in UmbracoConfig.For.UmbracoSettings().Content.ImageCrops.Crops)
            {
                var cropSize = typeCrops.CropSizes.FirstOrDefault(x => x.Alias == cropAlias);
                if(cropSize != null)
                    return new Models.ImageCropData() { Alias = cropSize.Alias, Height = cropSize.Height, Width = cropSize.Width };
            }*/
            
            return null;
        }
    }
}
