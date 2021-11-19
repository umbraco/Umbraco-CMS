﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a media picker property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MediaPicker3,
        EditorType.PropertyValue,
        "Media Picker",
        "mediapicker3",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaImage)]
    public class MediaPicker3PropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker3PropertyEditor" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MediaPicker3PropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPicker3ConfigurationEditor();

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new MediaPicker3PropertyValueEditor(Attribute);

        internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MediaPicker3PropertyValueEditor(DataEditorAttribute attribute)
                : base(attribute)
            { }

            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var value = property.GetValue(culture, segment);

                return Deserialize(value);
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                foreach (var dto in Deserialize(value))
                {
                    yield return new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Media, dto.MediaKey));
                }
            }

            internal static IEnumerable<MediaWithCropsDto> Deserialize(object value)
            {
                var rawJson = value is string str ? str : value?.ToString();
                if (string.IsNullOrWhiteSpace(rawJson))
                {
                    yield break;
                }

                if (!rawJson.DetectIsJson())
                {
                    // Old comma seperated UDI format
                    foreach (var udiStr in rawJson.Split(Constants.CharArrays.Comma))
                    {
                        if (GuidUdi.TryParse(udiStr, out var udi))
                        {
                            yield return new MediaWithCropsDto
                            {
                                Key = Guid.NewGuid(),
                                MediaKey = udi.Guid,
                                Crops = Enumerable.Empty<ImageCropperValue.ImageCropperCrop>(),
                                FocalPoint = new ImageCropperValue.ImageCropperFocalPoint
                                {
                                    Left = 0.5m,
                                    Top = 0.5m
                                }
                            };
                        }
                    }
                }
                else
                {
                    // New JSON format
                    foreach (var dto in JsonConvert.DeserializeObject<IEnumerable<MediaWithCropsDto>>(rawJson))
                    {
                        yield return dto;
                    }
                }
            }

            /// <summary>
            /// Model/DTO that represents the JSON that the MediaPicker3 stores.
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
}
