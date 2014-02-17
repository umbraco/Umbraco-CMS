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
        //Returns the collection of allowed crops on a given media alias type
        internal static Models.ImageCropDataSet GetConfigurationForType(string mediaTypeAlias)
        {
            var defaultModel = new Models.ImageCropDataSet();
            defaultModel.FocalPoint = new Models.ImageCropFocalPoint() { Left = 0.5M, Top = 0.5M };

            var configuredCrops = UmbracoConfig.For.UmbracoSettings().Content.ImageCrops.Crops.FirstOrDefault(x => x.MediaTypeAlias == mediaTypeAlias);
            if (configuredCrops == null || !configuredCrops.CropSizes.Any())
                return defaultModel;

            var crops = new Dictionary<string, Umbraco.Web.Models.ImageCropData>();
            foreach (var cropSize in configuredCrops.CropSizes)
                crops.Add(cropSize.Alias, new Models.ImageCropData() { Alias = cropSize.Alias, Height = cropSize.Height, Width = cropSize.Width });


            defaultModel.Crops = crops;
            return defaultModel;
        }

        internal static Umbraco.Web.Models.ImageCropData GetCrop(string mediaTypeAlias, string cropAlias){

            var _crops = GetConfigurationForType(mediaTypeAlias);
            
            if (_crops == null || _crops.Crops == null)
                return null;

            return _crops.Crops[cropAlias];
        }

        //this queries all crops configured
        internal static Umbraco.Web.Models.ImageCropData GetCrop(string cropAlias)
        {
            foreach (var typeCrops in UmbracoConfig.For.UmbracoSettings().Content.ImageCrops.Crops)
            {
                var cropSize = typeCrops.CropSizes.FirstOrDefault(x => x.Alias == cropAlias);
                if(cropSize != null)
                    return new Models.ImageCropData() { Alias = cropSize.Alias, Height = cropSize.Height, Width = cropSize.Width };
            }
            
            return null;
        }
    }
}
