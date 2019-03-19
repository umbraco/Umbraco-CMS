using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The media picker property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class MediaPickerValueConverter : PropertyValueConverterBase
    {
        // hard-coding "image" here but that's how it works at UI level too
        private const string ImageTypeAlias = "image";

        private readonly IPublishedModelFactory _publishedModelFactory;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MediaPickerValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IPublishedModelFactory publishedModelFactory)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ??
                                         throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _publishedModelFactory = publishedModelFactory;
        }

        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.MediaPicker);
        }

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
        {
            var isMultiple = IsMultipleDataType(propertyType.DataType);
            var isOnlyImages = IsOnlyImagesDataType(propertyType.DataType);

            return isMultiple
                ? isOnlyImages
                    ? typeof(IEnumerable<>).MakeGenericType(ModelType.For(ImageTypeAlias))
                    : typeof(IEnumerable<IPublishedContent>)
                : isOnlyImages
                    ? ModelType.For(ImageTypeAlias)
                    : typeof(IPublishedContent);
        }

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Snapshot;

        private bool IsMultipleDataType(PublishedDataType dataType)
        {
            var config = ConfigurationEditor.ConfigurationAs<MediaPickerConfiguration>(dataType.Configuration);
            return config.Multiple;
        }

        private bool IsOnlyImagesDataType(PublishedDataType dataType)
        {
            var config = ConfigurationEditor.ConfigurationAs<MediaPickerConfiguration>(dataType.Configuration);
            return config.OnlyImages;
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType,
            object source, bool preview)
        {
            if (source == null) return null;

            var nodeIds = source.ToString()
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Udi.Parse)
                .ToArray();
            return nodeIds;
        }

        public override object ConvertIntermediateToObject(IPublishedElement owner, PublishedPropertyType propertyType,
            PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            var isMultiple = IsMultipleDataType(propertyType.DataType);
            var isOnlyImages = IsOnlyImagesDataType(propertyType.DataType);

            var udis = (Udi[]) source;
            var mediaItems = isOnlyImages
                ? _publishedModelFactory.CreateModelList(ImageTypeAlias)
                : new List<IPublishedContent>();

            if (source == null) return isMultiple ? mediaItems : null;

            if (udis.Any())
            {
                foreach (var udi in udis)
                {
                    var guidUdi = udi as GuidUdi;
                    if (guidUdi == null) continue;
                    var item = _publishedSnapshotAccessor.PublishedSnapshot.Media.GetById(guidUdi.Guid);
                    if (item != null)
                        mediaItems.Add(item);
                }

                return isMultiple ? mediaItems : FirstOrDefault(mediaItems);
            }

            return source;
        }

        private object FirstOrDefault(IList mediaItems)
        {
            return mediaItems.Count == 0 ? null : mediaItems[0];
        }
    }
}
