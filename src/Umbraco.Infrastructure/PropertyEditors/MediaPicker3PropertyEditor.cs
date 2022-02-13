using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
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
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker3PropertyEditor" /> class.
        /// </summary>
        public MediaPicker3PropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            EditorType type = EditorType.PropertyValue)
            : base(dataValueEditorFactory, type)
        {
            _ioHelper = ioHelper;
        }


        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPicker3ConfigurationEditor(_ioHelper);

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<MediaPicker3PropertyValueEditor>(Attribute);

        internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IJsonSerializer _jsonSerializer;
            private readonly IDataTypeService _dataTypeService;

            public MediaPicker3PropertyValueEditor(
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper,
                DataEditorAttribute attribute,
                IDataTypeService dataTypeService)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
            {
                _jsonSerializer = jsonSerializer;
                _dataTypeService = dataTypeService;
            }

            public override object ToEditor(IProperty property, string culture = null, string segment = null)
            {
                var value = property.GetValue(culture, segment);

                var dtos = Deserialize(_jsonSerializer, value).ToList();

                var dataType = _dataTypeService.GetDataType(property.PropertyType.DataTypeId);
                if (dataType?.Configuration != null)
                {
                    var configuration = dataType.ConfigurationAs<MediaPicker3Configuration>();

                    foreach (var dto in dtos)
                    {
                        dto.ApplyConfiguration(configuration);
                    }
                }

                return dtos;
            }

            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value is JArray dtos)
                {
                    // Clean up redundant/default data
                    foreach (var dto in dtos.Values<JObject>())
                    {
                        MediaWithCropsDto.Prune(dto);
                    }

                    return dtos.ToString(Formatting.None);
                }

                return base.FromEditor(editorValue, currentValue);
            }

            ///<remarks>
            /// Note: no FromEditor() and ToEditor() methods
            /// We do not want to transform the way the data is stored in the DB and would like to keep a raw JSON string
            /// </remarks>

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                foreach (var dto in Deserialize(_jsonSerializer, value))
                {
                    yield return new UmbracoEntityReference(Udi.Create(Constants.UdiEntityType.Media, dto.MediaKey));
                }
            }

            internal static IEnumerable<MediaWithCropsDto> Deserialize(IJsonSerializer jsonSerializer,object value)
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
                        if (UdiParser.TryParse(udiStr, out GuidUdi udi))
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
                    foreach (var dto in jsonSerializer.Deserialize<IEnumerable<MediaWithCropsDto>>(rawJson))
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

                /// <summary>
                /// Applies the configuration to ensure only valid crops are kept and have the correct width/height.
                /// </summary>
                /// <param name="configuration">The configuration.</param>
                public void ApplyConfiguration(MediaPicker3Configuration configuration)
                {
                    var crops = new List<ImageCropperValue.ImageCropperCrop>();

                    var configuredCrops = configuration?.Crops;
                    if (configuredCrops != null)
                    {
                        foreach (var configuredCrop in configuredCrops)
                        {
                            var crop = Crops?.FirstOrDefault(x => x.Alias == configuredCrop.Alias);

                            crops.Add(new ImageCropperValue.ImageCropperCrop
                            {
                                Alias = configuredCrop.Alias,
                                Width = configuredCrop.Width,
                                Height = configuredCrop.Height,
                                Coordinates = crop?.Coordinates
                            });
                        }
                    }

                    Crops = crops;

                    if (configuration?.EnableLocalFocalPoint == false)
                    {
                        FocalPoint = null;
                    }
                }

                /// <summary>
                /// Removes redundant crop data/default focal point.
                /// </summary>
                /// <param name="value">The media with crops DTO.</param>
                /// <remarks>
                /// Because the DTO uses the same JSON keys as the image cropper value for crops and focal point, we can re-use the prune method.
                /// </remarks>
                public static void Prune(JObject value) => ImageCropperValue.Prune(value);
            }
        }
    }
}
