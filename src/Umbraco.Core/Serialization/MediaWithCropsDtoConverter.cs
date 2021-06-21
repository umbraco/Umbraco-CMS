using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Core.Serialization
{
    public class MediaWithCropsDtoConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var mediaWithCropsDto = new List<MediaWithCropsDto>();

            var mediaIds = value.ToString().Split(',');
            if (mediaIds.Any())
            {
                foreach (var mediaId in mediaIds)
                {
                    Guid mediaItemKey = default;
                    if (GuidUdi.TryParse(mediaId, out var guidUdi))
                    {
                        mediaItemKey = guidUdi.Guid;
                    }

                    if (mediaItemKey != default)
                    {
                        mediaWithCropsDto.Add(new MediaWithCropsDto()
                        {
                            Key = Guid.NewGuid(),
                            MediaKey = mediaItemKey,
                            Crops = new List<ImageCropperValue.ImageCropperCrop>(),
                            FocalPoint = new ImageCropperValue.ImageCropperFocalPoint {Left = 0.5m, Top = 0.5m}
                        });
                    }
                }
            }

            JToken.FromObject(mediaWithCropsDto).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
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
