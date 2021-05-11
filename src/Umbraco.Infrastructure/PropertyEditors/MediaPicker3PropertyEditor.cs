using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
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
        private readonly IIOHelper _ioHelper;

        public MediaPicker3PropertyEditor(
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            EditorType type = EditorType.PropertyValue)
            : base(
                loggerFactory,
                dataTypeService,
                localizationService,
                localizedTextService,
                shortStringHelper,
                jsonSerializer,
                type)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPicker3ConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => new MediaPicker3PropertyValueEditor(DataTypeService, LocalizationService, LocalizedTextService, ShortStringHelper,JsonSerializer,Attribute);

        internal class MediaPicker3PropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IJsonSerializer _jsonSerializer;

            public MediaPicker3PropertyValueEditor(
                IDataTypeService dataTypeService,
                ILocalizationService localizationService,
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                DataEditorAttribute attribute)
                : base(dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer, attribute)
            {
                _jsonSerializer = jsonSerializer;
            }

            ///<remarks>
            /// Note: no FromEditor() and ToEditor() methods
            /// We do not want to transform the way the data is stored in the DB and would like to keep a raw JSON string
            /// </remarks>

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                if (string.IsNullOrWhiteSpace(rawJson))
                    yield break;

                var mediaWithCropsDtos = _jsonSerializer.Deserialize<MediaPickerWithCropsValueConverter.MediaWithCropsDto[]>(rawJson);

                foreach (var mediaWithCropsDto in mediaWithCropsDtos)
                {
                    yield return new UmbracoEntityReference(GuidUdi.Create(Constants.UdiEntityType.Media, mediaWithCropsDto.MediaKey));
                }
            }

        }
    }
}
