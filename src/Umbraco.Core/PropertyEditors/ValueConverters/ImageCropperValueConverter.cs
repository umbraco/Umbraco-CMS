using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// This ensures that the cropper config (pre-values/crops) are merged in with the front-end value.
    /// </summary>
    [DefaultPropertyValueConverter(typeof (JsonValueConverter))] //this shadows the JsonValueConverter
    [PropertyValueType(typeof (JToken))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class ImageCropperValueConverter : JsonValueConverter
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.ImageCropperAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                JObject obj;
                try
                {
                    obj = JsonConvert.DeserializeObject<JObject>(sourceString);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ImageCropperValueConverter>("Could not parse the string " + sourceString + " to a json object", ex);
                    return sourceString;
                }

                //need to lookup the pre-values for this data type
                //TODO: Change all singleton access to use ctor injection in v8!!!
                var dt = ApplicationContext.Current.Services.DataTypeService.GetPreValuesCollectionByDataTypeId(propertyType.DataTypeId);

                if (dt != null && dt.IsDictionaryBased && dt.PreValuesAsDictionary.ContainsKey("crops"))
                {
                    var cropsString = dt.PreValuesAsDictionary["crops"].Value;
                    JArray crops;
                    try
                    {
                        crops = JsonConvert.DeserializeObject<JArray>(cropsString);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<ImageCropperValueConverter>("Could not parse the string " + cropsString + " to a json object", ex);
                        return sourceString;
                    }
                    obj["crops"] = crops;
                }
                
                return obj;
            }

            //it's not json, just return the string
            return sourceString;
        }
    }
}