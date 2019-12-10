using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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

        public ContentPickerPropertyEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, ILogger logger)
            : base(logger)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
        }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor();
        }

        protected override IDataValueEditor CreateValueEditor() => new ContentPickerPropertyValueEditor(_dataTypeService, _localizationService, Attribute);

        internal class ContentPickerPropertyValueEditor  : DataValueEditor, IDataValueReference
        {
            public ContentPickerPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, DataEditorAttribute attribute) : base(dataTypeService, localizationService, attribute)
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
