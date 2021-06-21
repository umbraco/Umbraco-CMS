using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Serialization;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MediaPickerWithCropsValueConverter : PropertyValueConverterBase
    {

        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly ILogger _logger;


        public MediaPickerWithCropsValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor, ILogger logger)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _logger = logger;
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

        /// <summary>
        /// Enusre this property value convertor is for the New Media Picker with Crops aka MediaPicker 3
        /// </summary>
        public override bool IsConverter(IPublishedPropertyType propertyType) => propertyType.EditorAlias.Equals(Core.Constants.PropertyEditors.Aliases.MediaPicker3);

        /// <summary>
        /// Check if the raw JSON value is not an empty array
        /// </summary>
        public override bool? IsValue(object value, PropertyValueLevel level) => value?.ToString() != "[]";

        /// <summary>
        /// What C# model type does the raw JSON return for Models & Views
        /// </summary>
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        {
            // Check do we want to return IPublishedContent collection still or a NEW model ?
            var isMultiple = IsMultipleDataType(propertyType.DataType);
            return isMultiple
                    ? typeof(IEnumerable<MediaWithCrops>)
                    : typeof(MediaWithCrops);
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview) => source?.ToString();

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            var mediaItems = new List<MediaWithCrops>();
            var isMultiple = IsMultipleDataType(propertyType.DataType);
            if (inter == null)
            {
                return isMultiple ? mediaItems: null;
            }

            var value = inter.ToString();
            if (value.DetectIsJson() == false)
            {
                // If the value is not yet JSON we'll try to convert it
                try
                {
                    value = JsonConvert.SerializeObject(value, Formatting.Indented, new MediaWithCropsDtoConverter());
                }
                catch (Exception ex)
                {
                    _logger.Error<MediaWithCropsDtoConverter>(ex, $"Could not convert data to Media Picker v3 format, the data stored is: {value}");
                }
            }

            var dtos = JsonConvert.DeserializeObject<IEnumerable<MediaWithCropsDto>>(value);

            foreach(var media in dtos)
            {
                var item = _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(media.MediaKey);
                if (item != null)
                {
                    mediaItems.Add(new MediaWithCrops
                    {
                        MediaItem = item,
                        LocalCrops = new ImageCropperValue
                        {
                            Crops = media.Crops,
                            FocalPoint = media.FocalPoint,
                            Src = item.Url()
                        }
                    });
                }
            }

            return isMultiple ? mediaItems : FirstOrDefault(mediaItems);
        }

        /// <summary>
        /// Is the media picker configured to pick multiple media items
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private bool IsMultipleDataType(PublishedDataType dataType)
        {
            var config = dataType.ConfigurationAs<MediaPicker3Configuration>();
            return config.Multiple;
        }

        private object FirstOrDefault(IList mediaItems)
        {
            return mediaItems.Count == 0 ? null : mediaItems[0];
        }
    }
}
