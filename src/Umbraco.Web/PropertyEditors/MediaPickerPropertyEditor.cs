using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a media picker property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MediaPicker,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Media Picker",
        "mediapicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaImage)]
    public class MediaPickerPropertyEditor : DataEditor
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerPropertyEditor"/> class.
        /// </summary>
        public MediaPickerPropertyEditor(
            ILogger logger,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            IIOHelper ioHelper,
            IShortStringHelper shortStringHelper,
            ILocalizedTextService localizedTextService)
            : base(logger, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new MediaPickerConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => new MediaPickerPropertyValueEditor(_dataTypeService, _localizationService, Attribute);

        internal class MediaPickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MediaPickerPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, DataEditorAttribute attribute) : base(dataTypeService,localizationService, Current.Services.TextService,Current.ShortStringHelper,attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString)) yield break;


                if (UdiParser.TryParse(asString, out var udi))
                    yield return new UmbracoEntityReference(udi);
            }
        }
    }


}
