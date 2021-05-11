
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MediaPickerWithCropsValueConverter : PropertyValueConverterBase
    {

        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IPublishedUrlProvider _publishedUrlProvider;

        public MediaPickerWithCropsValueConverter(
            IPublishedSnapshotAccessor publishedSnapshotAccessor,
            IPublishedUrlProvider publishedUrlProvider)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _publishedUrlProvider = publishedUrlProvider;
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

            var dtos = JsonConvert.DeserializeObject<IEnumerable<MediaWithCropsDto>>(inter.ToString());

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
                            Src = item.Url(_publishedUrlProvider)
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


        /// <summary>
        /// Model/DTO that represents the JSON that the MediaPicker3 stores
        /// </summary>
        [DataContract]
        internal class MediaWithCropsDto
        {
            [DataMember(Name = "key")]
            public Guid Key { get; set; }

            [DataMember(Name = "mediaKey")]
            public Guid MediaKey { get; set; }

            [DataMember(Name = "crops")]
            public IEnumerable<ImageCropperValue.ImageCropperCrop> Crops { get; set; }

            [DataMember(Name = "focalPoint")]
            public ImageCropperValue.ImageCropperFocalPoint FocalPoint { get; set; }
        }
    }
}
