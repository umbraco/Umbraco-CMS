namespace Umbraco.Web
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using Newtonsoft.Json;

    using Umbraco.Core;
    using Umbraco.Core.Logging;
    using Umbraco.Web.Models;

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

        internal static string GetCropBaseUrl(this ImageCropDataSet cropDataSet, string cropAlias, bool preferFocalPoint)
        {
            var cropUrl = new StringBuilder();

            var crop = cropDataSet.GetCrop(cropAlias);

            // if crop alias has been specified but not found in the Json we should return null
            if (string.IsNullOrEmpty(cropAlias) == false && crop == null)
            {
                return null;
            }
            if ((preferFocalPoint && cropDataSet.HasFocalPoint()) || (crop != null && crop.Coordinates == null && cropDataSet.HasFocalPoint()) || (string.IsNullOrEmpty(cropAlias) && cropDataSet.HasFocalPoint()))
            {
                cropUrl.Append("?center=" + cropDataSet.FocalPoint.Top.ToString(CultureInfo.InvariantCulture) + "," + cropDataSet.FocalPoint.Left.ToString(CultureInfo.InvariantCulture));
                cropUrl.Append("&mode=crop");
            }
            else if (crop != null && crop.Coordinates != null && preferFocalPoint == false)
            {
                cropUrl.Append("?crop=");
                cropUrl.Append(crop.Coordinates.X1.ToString(CultureInfo.InvariantCulture)).Append(",");
                cropUrl.Append(crop.Coordinates.Y1.ToString(CultureInfo.InvariantCulture)).Append(",");
                cropUrl.Append(crop.Coordinates.X2.ToString(CultureInfo.InvariantCulture)).Append(",");
                cropUrl.Append(crop.Coordinates.Y2.ToString(CultureInfo.InvariantCulture));
                cropUrl.Append("&cropmode=percentage");
            }
            else
            {
                cropUrl.Append("?anchor=center");
                cropUrl.Append("&mode=crop");
            }
            return cropUrl.ToString();
        }
    }
}
