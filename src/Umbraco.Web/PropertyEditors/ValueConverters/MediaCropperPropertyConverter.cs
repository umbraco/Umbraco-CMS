// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//  The media picker 2 value converter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Extensions;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The media picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class MediaCropperPropertyConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        private readonly IDataTypeService _dataTypeService;

        //TODO: Remove this ctor in v8 since the other one will use IoC
        public MediaCropperPropertyConverter()
            : this(ApplicationContext.Current.Services.DataTypeService)
        {
        }

        public MediaCropperPropertyConverter(IDataTypeService dataTypeService)
        {
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            _dataTypeService = dataTypeService;
        }

        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias) ? typeof(IEnumerable<ImageCropDataSet>) : typeof(ImageCropDataSet);
        }

        public PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType, PropertyCacheValue cacheValue)
        {
            PropertyCacheLevel returnLevel;
            switch (cacheValue)
            {
                case PropertyCacheValue.Object:
                    returnLevel = PropertyCacheLevel.ContentCache;
                    break;
                case PropertyCacheValue.Source:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                case PropertyCacheValue.XPath:
                    returnLevel = PropertyCacheLevel.Content;
                    break;
                default:
                    returnLevel = PropertyCacheLevel.None;
                    break;
            }

            return returnLevel;
        }

        /// <summary>
        /// The is multiple data type.
        /// </summary>
        /// <param name="dataTypeId">
        /// The data type id.
        /// </param>
        /// <param name="propertyEditorAlias"></param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsMultipleDataType(int dataTypeId, string propertyEditorAlias)
        {
            // GetPreValuesCollectionByDataTypeId is cached at repository level;
            // still, the collection is deep-cloned so this is kinda expensive,
            // better to cache here + trigger refresh in DataTypeCacheRefresher

            return Storages.GetOrAdd(dataTypeId, id =>
            {
                var preVals = _dataTypeService.GetPreValuesCollectionByDataTypeId(id).PreValuesAsDictionary;

                if (preVals.ContainsKey("multiPicker"))
                {
                    var preValue = preVals
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.Value.TryConvertTo<bool>().Result;
                }

                //in some odd cases, the pre-values in the db won't exist but their default pre-values contain this key so check there 
                var propertyEditor = PropertyEditorResolver.Current.GetByAlias(propertyEditorAlias);
                if (propertyEditor != null)
                {
                    var preValue = propertyEditor.DefaultPreValues
                        .FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase))
                        .Value;

                    return preValue != null && preValue.TryConvertTo<bool>().Result;
                }

                return false;
            });
        }

        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MediaCropperAlias))
                return true;

            return false;
        }


        internal static void MergePreValues(JArray currentValues, IDataTypeService dataTypeService, int dataTypeId)
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

                foreach (var currentValue in currentValues)
                {
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
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                JArray obj;
                try
                {
                    obj = JsonConvert.DeserializeObject<JArray>(sourceString, new JsonSerializerSettings
                    {
                        Culture = CultureInfo.InvariantCulture,
                        FloatParseHandling = FloatParseHandling.Decimal
                    });
                }
                catch (Exception ex)
                {
                    LogHelper.Error<MediaCropperPropertyConverter>("Could not parse the string " + sourceString + " to a json object", ex);
                    return sourceString;
                }

                MergePreValues(obj, _dataTypeService, propertyType.DataTypeId);

                return obj;
            }

            //it's not json, just return the string
            return sourceString;
        }

        /// <summary>
        /// Convert the source nodeId into a IPublishedContent (or DynamicPublishedContent)
        /// </summary>
        /// <param name="propertyType">
        /// The published property type.
        /// </param>
        /// <param name="source">
        /// The value of the property
        /// </param>
        /// <param name="preview">
        /// The preview.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public override object ConvertSourceToObject(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null)
            {
                return null;
            }

            var cropDataSets = ((JArray)source).ToObject<IEnumerable<ImageCropDataSet>>();

            if (IsMultipleDataType(propertyType.DataTypeId, propertyType.PropertyEditorAlias))
            {
                return cropDataSets;
            }
            else
            {
                return cropDataSets.FirstOrDefault();
            }
        }

        private static readonly ConcurrentDictionary<int, bool> Storages = new ConcurrentDictionary<int, bool>();

        internal static void ClearCaches()
        {
            Storages.Clear();
        }
    }
}
