using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

        /// <summary>
        /// Enusre this property value convertor is for the New Media Picker with Crops aka MediaPicker 3
        /// </summary>
        public override bool IsConverter(IPublishedPropertyType propertyType) => propertyType.EditorAlias.Equals(Core.Constants.PropertyEditors.Aliases.MediaPicker3);

        /// <summary>
        /// Check if the raw JSON value is not an empty array
        /// </summary>
        public override bool? IsValue(object value, PropertyValueLevel level)
            => value?.ToString() is string stringValue &&
                stringValue != null &&
                !string.IsNullOrEmpty(stringValue) &&
                stringValue != "[]";

        /// <summary>
        /// What C# model type does the raw JSON return for Models & Views
        /// </summary>
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => IsMultipleDataType(propertyType.DataType)
                ? typeof(IEnumerable<MediaWithCrops>)
                : typeof(MediaWithCrops);

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object inter, bool preview)
        {
            var isMultiple = IsMultipleDataType(propertyType.DataType);
            if (inter == null)
            {
                // Short-circuit on empty value
                return isMultiple ? Enumerable.Empty<MediaWithCrops>() : null;
            }

            var mediaItems = new List<MediaWithCrops>();
            var dtos = MediaPicker3PropertyEditor.MediaPicker3PropertyValueEditor.Deserialize(inter);

            foreach (var dto in dtos)
            {
                var mediaItem = _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(preview, dto.MediaKey);
                if (mediaItem != null)
                {
                    mediaItems.Add(new MediaWithCrops
                    {
                        MediaItem = mediaItem,
                        LocalCrops = new ImageCropperValue
                        {
                            Crops = dto.Crops,
                            FocalPoint = dto.FocalPoint,
                            Src = mediaItem.Url()
                        }
                    });

                    if (!isMultiple)
                    {
                        // Short-circuit on single item
                        break;
                    }
                }
            }

            return isMultiple ? mediaItems : mediaItems.FirstOrDefault();
        }

        /// <summary>
        /// Is the media picker configured to pick multiple media items
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        private bool IsMultipleDataType(PublishedDataType dataType) => dataType.ConfigurationAs<MediaPicker3Configuration>().Multiple;
    }
}
