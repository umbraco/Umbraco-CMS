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
                catch { }
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
                catch(Exception ex)
                {
                    var e = ex;
                }
            }

            return imageCrops;
        }



        internal static bool HasPropertyAndValue(this IPublishedContent publishedContent, string propertyAlias)
        {
            try
            {
               
                if (propertyAlias != null && publishedContent.HasProperty(propertyAlias)
                    && publishedContent.HasValue(propertyAlias))
                {
                    var propertyAliasValue = publishedContent.GetPropertyValue<string>(propertyAlias);
                    if (propertyAliasValue.DetectIsJson() && propertyAliasValue.Length <= 2)
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warn<IPublishedContent>("The cache unicorn is not happy with node id: " + publishedContent.Id + " - http://issues.umbraco.org/issue/U4-4146");
                
                var cropsProperty = publishedContent.Properties.FirstOrDefault(x => x.PropertyTypeAlias == propertyAlias);

                if (cropsProperty != null && !string.IsNullOrEmpty(cropsProperty.Value.ToString()))
                {
                    var propertyAliasValue = cropsProperty.Value.ToString();
                    if (propertyAliasValue.DetectIsJson() && propertyAliasValue.Length <= 2)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        internal static ImageCropData GetCrop(this ImageCropDataSet dataset, string cropAlias)
        {
            if (dataset == null || dataset.Crops == null || !dataset.Crops.Any())
                return null;

            return dataset.Crops.GetCrop(cropAlias);
        }

        internal static ImageCropData GetCrop(this IEnumerable<ImageCropData> dataset, string cropAlias){
            if (dataset == null || !dataset.Any())
                return null;

            if (string.IsNullOrEmpty(cropAlias))
                return dataset.FirstOrDefault();

            return dataset.FirstOrDefault(x => x.Alias.ToLowerInvariant() == cropAlias.ToLowerInvariant());
        }



        internal static bool HasPropertyAndValueAndCrop(this IPublishedContent publishedContent, string propertyAlias, string cropAlias)
        {
            try
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
                    var selectedCrop = allTheCrops.GetCrop(cropAlias);

                    if (selectedCrop != null)
                        return true;

                    return false;

               }
            }
            catch (Exception ex)
            {
                LogHelper.Warn<IPublishedContent>("The cache unicorn is not happy with node id: " + publishedContent.Id + " - http://issues.umbraco.org/issue/U4-4146");
                var cropsProperty = publishedContent.Properties.FirstOrDefault(x => x.PropertyTypeAlias == propertyAlias);

                if (cropsProperty != null && !string.IsNullOrEmpty(cropsProperty.Value.ToString()))
                {
                    var propertyAliasValue = cropsProperty.Value.ToString();
                    if (propertyAliasValue.DetectIsJson() && propertyAliasValue.Length <= 2)
                    {
                        return false;
                    }
                    var allTheCrops = propertyAliasValue.SerializeToCropDataSet();
                    return allTheCrops.GetCrop(cropAlias) != null;
                }
            }
            return false;
        }

        internal static string GetPropertyValueHack(this IPublishedContent publishedContent, string propertyAlias)
        {
            string propertyValue = null;
            try
            {
                if (propertyAlias != null && publishedContent.HasProperty(propertyAlias)
                    && publishedContent.HasValue(propertyAlias))
                {
                    propertyValue = publishedContent.GetPropertyValue<string>(propertyAlias);
                }
            }
            catch (Exception ex)
            {
                var cropsProperty = publishedContent.Properties.FirstOrDefault(x => x.PropertyTypeAlias == propertyAlias);
                if (cropsProperty != null)
                {
                    propertyValue = cropsProperty.Value.ToString();
                }
            }
            return propertyValue;
        }
    }
}
