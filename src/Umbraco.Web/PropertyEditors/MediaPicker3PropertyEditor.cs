using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
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
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker3PropertyEditor"/> class.
        /// </summary>
        public MediaPicker3PropertyEditor(ILogger logger)
            : base(logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPicker3ConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MediaPicker3PropertyValueEditor(Attribute, _logger);

        internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly ILogger _logger;

            ///<remarks>
            /// Note: no FromEditor() method
            /// Only transform the way the data is stored if it's not already JSON
            /// </remarks>
            public MediaPicker3PropertyValueEditor(DataEditorAttribute attribute, ILogger logger) : base(attribute)
            {
                _logger = logger;
            }

            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null,
                string segment = null)
            {
                var value = property.GetValue(culture, segment)?.ToString();
                if (value != null && value.DetectIsJson() == false)
                {
                    // If the value is not yet JSON we'll try to convert it
                    try
                    {
                        value = JsonConvert.SerializeObject(value, Formatting.Indented, new MediaWithCropsDtoConverter());
                        property.SetValue(value);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error<MediaWithCropsDtoConverter>(ex, $"Could not convert data to Media Picker v3 format, the data stored is: {value}");
                    }
                }

                // If it is already JSON, just continue as normal
                return base.ToEditor(property, dataTypeService, culture, segment);
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                if (rawJson.IsNullOrWhiteSpace())
                    yield break;

                var mediaWithCropsDtos = JsonConvert.DeserializeObject<MediaWithCropsDto[]>(rawJson);

                foreach (var mediaWithCropsDto in mediaWithCropsDtos)
                {
                    yield return new UmbracoEntityReference(GuidUdi.Create(Constants.UdiEntityType.Media, mediaWithCropsDto.MediaKey));
                }
            }
        }
    }
}
