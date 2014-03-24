using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    internal static class ImageCropperBaseExtensions
    {

        internal static ImageCropData GetImageCrop(this string json, string id)
        {
            var ic = new ImageCropData();
            if (json.DetectIsJson())
            {
                try
                {
                    var imageCropperSettings = JsonConvert.DeserializeObject<List<ImageCropData>>(json);
                    ic = imageCropperSettings.GetCrop(id);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(typeof(ImageCropperBaseExtensions), "Could not parse the json string: " + json, ex);
                }
            }
            return ic;
        }

        internal static ImageCropDataSet SerializeToCropDataSet(this string json)
        {
            var imageCrops = new ImageCropDataSet();
            if (json.DetectIsJson())
            {
                try
                {
                    imageCrops = JsonConvert.DeserializeObject<ImageCropDataSet>(json);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(typeof(ImageCropperBaseExtensions), "Could not parse the json string: " + json, ex);
                }
            }

            return imageCrops;
        }

        internal static ImageCropData GetCrop(this ImageCropDataSet dataset, string cropAlias)
        {
            if (dataset == null || dataset.Crops == null || !dataset.Crops.Any())
                return null;

            return dataset.Crops.GetCrop(cropAlias);
        }

        internal static ImageCropData GetCrop(this IEnumerable<ImageCropData> dataset, string cropAlias)
        {
            if (dataset == null || !dataset.Any())
                return null;

            if (string.IsNullOrEmpty(cropAlias))
                return dataset.FirstOrDefault();

            return dataset.FirstOrDefault(x => x.Alias.ToLowerInvariant() == cropAlias.ToLowerInvariant());
        }



        internal static bool HasPropertyAndValueAndCrop(this IPublishedContent publishedContent, string propertyAlias, string cropAlias)
        {
            if (propertyAlias != null && publishedContent.HasProperty(propertyAlias)
                && publishedContent.HasValue(propertyAlias))
            {
                var propertyAliasValue = publishedContent.GetPropertyValue<string>(propertyAlias);
                if (propertyAliasValue.DetectIsJson() && propertyAliasValue.Length <= 2)
                {
                    return false;
                }
                var allTheCrops = propertyAliasValue.SerializeToCropDataSet();
                if (allTheCrops != null && allTheCrops.Crops.Any())
                {
                    var crop = cropAlias != null
                                    ? allTheCrops.Crops.First(x => x.Alias.ToLowerInvariant() == cropAlias.ToLowerInvariant())

                                    : allTheCrops.Crops.First();
                    if (crop != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

    }
}
