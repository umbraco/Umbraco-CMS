using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

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
        private readonly IDataTypeService _dataTypeService;

        public ImageCropperValueConverter()
        {
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
        }

        public ImageCropperValueConverter(IDataTypeService dataTypeService)
        {
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _dataTypeService = dataTypeService;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.ImageCropperAlias);
        }

        internal static void MergePreValues(JObject currentValue, IDataTypeService dataTypeService, int dataTypeId)
        {
            //need to lookup the pre-values for this data type
            //TODO: Change all singleton access to use ctor injection in v8!!!
            var dt = dataTypeService.GetPreValuesCollectionByDataTypeId(dataTypeId);

            if (dt != null && dt.IsDictionaryBased && dt.PreValuesAsDictionary.ContainsKey("crops"))
            {
                var cropsString = dt.PreValuesAsDictionary["crops"].Value;
                JArray preValueCrops;
                try
                {
                    preValueCrops = JsonConvert.DeserializeObject<JArray>(cropsString);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ImageCropperValueConverter>("Could not parse the string " + cropsString + " to a json object", ex);
                    return;
                }

                //now we need to merge the crop values - the alias + width + height comes from pre-configured pre-values,
                // however, each crop can store it's own coordinates

                JArray existingCropsArray;
                if (currentValue["crops"] != null)
                {
                    existingCropsArray = (JArray)currentValue["crops"];
                }
                else
                {
                    currentValue["crops"] = existingCropsArray = new JArray();
                }

                foreach (var preValueCrop in preValueCrops.Where(x => x.HasValues))
                {
                    var found = existingCropsArray.FirstOrDefault(x =>
                    {
                        if (x.HasValues && x["alias"] != null)
                        {
                            return x["alias"].Value<string>() == preValueCrop["alias"].Value<string>();
                        }
                        return false;
                    });
                    if (found != null)
                    {
                        found["width"] = preValueCrop["width"];
                        found["height"] = preValueCrop["height"];
                    }
                    else
                    {
                        existingCropsArray.Add(preValueCrop);
                    }
                }
            }
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
                    obj = JsonConvert.DeserializeObject<JObject>(sourceString, new JsonSerializerSettings
                    {
                        Culture = CultureInfo.InvariantCulture,
                        FloatParseHandling = FloatParseHandling.Decimal
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ImageCropperValueConverter>("Could not parse the string " + sourceString + " to a json object", ex);
                    return sourceString;
                }

                MergePreValues(obj, _dataTypeService, propertyType.DataTypeId);

                return obj;
            }

            //it's not json, just return the string
            return sourceString;
        }
    }
}