using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [DefaultPropertyValueConverter]
    public class MediaPickerWithCropsValueConverter : PropertyValueConverterBase
    {
        // JSON is an array of the following JSON
        /*
        "key": "d1e9c080-5aef-48f6-9b37-47e84de7ad8b",
        "mediaKey": "80e2507b-c5f9-4c8a-ab14-5dae717a49b3",
        "crops": [
            {
                "alias": "square",
                "label": "Square",
                "width": 200,
                "height": 200,
                "coordinates": {
                    "x1": 0.4489663034789123,
                    "y1": 0.7192332259400422,
                    "x2": 0.39324276949939146,
                    "y2": 0
                }
            },
            ...
        ]
        "focalPoint": {
            "left": 0.846,
            "top": 0.7964285714285714
        }
        */

        private IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MediaPickerWithCropsValueConverter(IPublishedSnapshotAccessor publishedSnapshotAccessor)
        {
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
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
            return !config.SingleMode;
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

        /// <summary>
        /// Model used in Razor Views for rendering
        /// </summary>
        public class MediaWithCrops
        {
            public IPublishedContent MediaItem { get; set; }

            public ImageCropperValue LocalCrops { get; set; }
        }
    }
}
