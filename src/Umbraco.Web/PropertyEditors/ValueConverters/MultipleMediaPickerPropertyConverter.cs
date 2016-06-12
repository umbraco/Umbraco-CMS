// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultipleMediaPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   The multiple media picker property editor converter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The multiple media picker property value converter.
    /// </summary>
    public class MultipleMediaPickerPropertyConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        /// <summary>
        /// Checks if this converter can convert the property editor and registers if it can.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MultipleMediaPickerAlias);
        }

        /// <summary>
        /// Convert the raw string into a nodeId integer array or a single integer
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
        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (IsMultipleDataType(propertyType.DataTypeId))
            {
                var nodeIds =
                    source.ToString()
                        .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();
                return nodeIds;
            }

            var attemptConvertInt = source.TryConvertTo<int>();
            if (attemptConvertInt.Success)
            {
                return attemptConvertInt.Result;
            }
            else
            {
                var nodeIds =
                   source.ToString()
                       .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(int.Parse)
                       .ToArray();

                if (nodeIds.Length > 0)
                {
                    var error =
                        string.Format(
                            "Data type \"{0}\" is not set to allow multiple items but appears to contain multiple items, check the setting and save the data type again",
                            ApplicationContext.Current.Services.DataTypeService.GetDataTypeDefinitionById(
                                propertyType.DataTypeId).Name);

                    LogHelper.Warn<MultipleMediaPickerPropertyConverter>(error);
                    throw new Exception(error);
                }
            }

            return null;
        }

        /// <summary>
        /// Convert the source nodeId into a IPublishedContent or IEnumerable of IPublishedContent (or DynamicPublishedContent) depending on data type setting
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

            if (UmbracoContext.Current == null)
            {
                return null;
            }

            var umbHelper = new UmbracoHelper(UmbracoContext.Current);

            if (IsMultipleDataType(propertyType.DataTypeId))
            {
                var nodeIds = (int[])source;
                var multiMediaPicker = Enumerable.Empty<IPublishedContent>();
                if (nodeIds.Length > 0)
                {
                    multiMediaPicker =  umbHelper.TypedMedia(nodeIds).Where(x => x != null);
                }

                return multiMediaPicker;
            }

            // single value picker
            var nodeId = (int)source;
            
            return umbHelper.TypedMedia(nodeId);
        }

        /// <summary>
        /// The get property cache level.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <param name="cacheValue">
        /// The cache value.
        /// </param>
        /// <returns>
        /// The <see cref="PropertyCacheLevel"/>.
        /// </returns>
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
        /// The get property value type.
        /// </summary>
        /// <param name="propertyType">
        /// The property type.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        public virtual Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataTypeId) ? typeof(IEnumerable<IPublishedContent>) : typeof(IPublishedContent);
        }

        /// <summary>
        /// The is multiple data type.
        /// </summary>
        /// <param name="dataTypeId">
        /// The data type id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsMultipleDataType(int dataTypeId)
        {
            var dts = ApplicationContext.Current.Services.DataTypeService;
            var multiPickerPreValue =
                dts.GetPreValuesCollectionByDataTypeId(dataTypeId)
                    .PreValuesAsDictionary.FirstOrDefault(
                        x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase)).Value;

            return multiPickerPreValue != null && multiPickerPreValue.Value.TryConvertTo<bool>().Result;
        }
    }
}