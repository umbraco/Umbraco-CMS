using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.ContentPicker,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Content Picker",
        "contentpicker",
        ValueType = ValueTypes.String,
        Group = Constants.PropertyEditors.Groups.Pickers)]
    public class ContentPickerPropertyEditor : DataEditor
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly IIOHelper _ioHelper;

        public ContentPickerPropertyEditor(
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILogger logger,
            IIOHelper ioHelper,
            IShortStringHelper shortStringHelper)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService,Current.Services.TextService, shortStringHelper)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
        }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor(_ioHelper);
        }

        protected override IDataValueEditor CreateValueEditor() => new ContentPickerPropertyValueEditor(_dataTypeService, _localizationService, ShortStringHelper, Attribute);

        internal class ContentPickerPropertyValueEditor  : DataValueEditor, IDataValueReference
        {
            public ContentPickerPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, IShortStringHelper shortStringHelper, DataEditorAttribute attribute) : base(dataTypeService, localizationService,Current.Services.TextService, shortStringHelper, attribute)
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
