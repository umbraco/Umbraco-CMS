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

        public MediaPickerWithCropsValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType) => PropertyCacheLevel.Snapshot;

        public override bool IsConverter(IPublishedPropertyType propertyType) => propertyType.EditorAlias.Equals(Core.Constants.PropertyEditors.Aliases.MediaPicker3);

        public override bool? IsValue(object value, PropertyValueLevel level)
            => value?.ToString() is string stringValue &&
                stringValue != null &&
                !string.IsNullOrEmpty(stringValue) &&
                stringValue != "[]";

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

        private bool IsMultipleDataType(PublishedDataType dataType) => dataType.ConfigurationAs<MediaPicker3Configuration>().Multiple;
    }
}
