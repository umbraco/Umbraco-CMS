// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaPickerPropertyConverter.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//  The legacy media picker value converter
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
using Umbraco.Web.Extensions;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The media picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class MediaPickerPropertyConverter : PropertyValueConverterBase, IPropertyValueConverterMeta
    {
        public Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            return IsMultipleDataType(propertyType.DataTypeId) ? typeof(IEnumerable<IPublishedContent>) : typeof(IPublishedContent);
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
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool IsMultipleDataType(int dataTypeId)
        {
            var multipleItems = false;

            try
            {
                var dts = ApplicationContext.Current.Services.DataTypeService;

                var multiPickerPreValues =
                    dts.GetPreValuesCollectionByDataTypeId(dataTypeId).PreValuesAsDictionary;

                var multiPickerPreValue = multiPickerPreValues.FirstOrDefault(x => string.Equals(x.Key, "multiPicker", StringComparison.InvariantCultureIgnoreCase)).Value;

                var attemptConvert = multiPickerPreValue.Value.TryConvertTo<bool>();

                if (attemptConvert.Success)
                {
                    multipleItems = attemptConvert.Result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MediaPickerPropertyConverter>(string.Format("Error finding multipleItems data type prevalue on data type Id {0}", dataTypeId), ex);
            }

            return multipleItems;
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
            // ** not sure if we want to convert the legacy media picker or not **
            if (propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MediaPickerAlias))
                return false;

            return propertyType.PropertyEditorAlias.Equals(Constants.PropertyEditors.MediaPicker2Alias);
        }

        /// <summary>
        /// Convert the raw string into a nodeId integer
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
            var nodeIds = source.ToString()
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Udi.Parse)
                .ToArray();
            return nodeIds;
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

            var udis = (Udi[])source;
            var mediaItems = new List<IPublishedContent>();
            if (udis.Any())
            {
                foreach (var udi in udis)
                {
                    var item = udi.ToPublishedContent();
                    if (item != null)
                        mediaItems.Add(item);
                }
                if (IsMultipleDataType(propertyType.DataTypeId))
                {
                    return mediaItems;
                }
                else
                {
                    return mediaItems.FirstOrDefault();
                }
            }

            return source;
        }
    }
}