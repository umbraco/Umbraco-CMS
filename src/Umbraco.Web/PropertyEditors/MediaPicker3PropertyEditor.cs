using System.Collections.Generic;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors.ValueConverters;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a media picker property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MediaPicker3,
        EditorType.PropertyValue,
        "Media Picker v3",
        "mediapicker3",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaImage)]
    public class MediaPicker3PropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker3PropertyEditor"/> class.
        /// </summary>
        public MediaPicker3PropertyEditor(ILogger logger)
            : base(logger)
        {
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPicker3ConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MediaPicker3PropertyValueEditor(Attribute);

        internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
        {
            ///<remarks>
            /// Note: no FromEditor() and ToEditor() methods
            /// We do not want to transform the way the data is stored in the DB and would like to keep a raw JSON string
            /// </remarks>
            public MediaPicker3PropertyValueEditor(DataEditorAttribute attribute) : base(attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                if (rawJson.IsNullOrWhiteSpace())
                    yield break;

                var mediaWithCropsDtos = JsonConvert.DeserializeObject<MediaPickerWithCropsValueConverter.MediaWithCropsDto[]>(rawJson);

                foreach (var mediaWithCropsDto in mediaWithCropsDtos)
                {
                    yield return new UmbracoEntityReference(GuidUdi.Create(Constants.UdiEntityType.Media, mediaWithCropsDto.MediaKey));
                }
            }

        }
    }
}
