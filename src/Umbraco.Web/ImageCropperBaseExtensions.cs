using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web
{
    internal static class ImageCropperBaseExtensions
    {
        internal static bool IsJson(this string input)
        {
            input = input.Trim();
            return input.StartsWith("{") && input.EndsWith("}") || input.StartsWith("[") && input.EndsWith("]");
        }

        internal static ImageCropData GetImageCrop(this string json, string id)
        {
            var ic = new ImageCropData();
            if (IsJson(json))
            {
                try
                {
                    var imageCropperSettings = JsonConvert.DeserializeObject<List<ImageCropData>>(json);
                    ic = imageCropperSettings.First(p => p.Alias == id);
                }
                catch { }
            }
            return ic;
        }

        internal static ImageCropDataSet SerializeToCropDataSet(this string json)
        {
            var imageCrops = new ImageCropDataSet();
            if (IsJson(json))
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
                    if (propertyAliasValue.IsJson() && propertyAliasValue.Length <= 2)
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
                    if (propertyAliasValue.IsJson() && propertyAliasValue.Length <= 2)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        internal static bool HasPropertyAndValueAndCrop(this IPublishedContent publishedContent, string propertyAlias, string cropAlias)
        {
            try
            {
                if (propertyAlias != null && publishedContent.HasProperty(propertyAlias)
                    && publishedContent.HasValue(propertyAlias))
                {
                    var propertyAliasValue = publishedContent.GetPropertyValue<string>(propertyAlias);
                    if (propertyAliasValue.IsJson() && propertyAliasValue.Length <= 2)
                    {
                        return false;
                    }
                    var allTheCrops = propertyAliasValue.SerializeToCropDataSet();
                    if (allTheCrops != null && allTheCrops.Crops.Any())
                    {
                        var crop = cropAlias != null
                                       ? allTheCrops.Crops.First(x => x.Alias ==cropAlias)
                                       : allTheCrops.Crops.First();
                        if (crop != null)
                        {
                            return true;
                        }
                    }
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
                    if (propertyAliasValue.IsJson() && propertyAliasValue.Length <= 2)
                    {
                        return false;
                    }
                    var allTheCrops = propertyAliasValue.SerializeToCropDataSet();
                    if (allTheCrops != null && allTheCrops.Crops.Any())
                    {
                        var crop = cropAlias != null
                                       ? allTheCrops.Crops.First(x => x.Alias ==cropAlias)
                                       : allTheCrops.Crops.First();
                        if (crop != null)
                        {
                            return true;
                        }
                    }
                    return false;
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
