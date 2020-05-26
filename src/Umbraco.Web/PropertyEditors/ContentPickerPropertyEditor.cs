using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

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
        public ContentPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor();
        }

        protected override IDataValueEditor CreateValueEditor() => new ContentPickerPropertyValueEditor(Attribute);

        internal class ContentPickerPropertyValueEditor  : DataValueEditor, IDataValueReference
        {
            public ContentPickerPropertyValueEditor(DataEditorAttribute attribute) : base(attribute)
            {
            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value is string str ? str : value?.ToString();

                if (string.IsNullOrEmpty(asString)) yield break;

                if (Udi.TryParse(asString, out var udi))
                    yield return new UmbracoEntityReference(udi);
            }
        }
    }
}
