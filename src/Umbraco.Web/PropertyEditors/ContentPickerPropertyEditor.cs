using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.ContentPicker, EditorType.PropertyValue | EditorType.MacroParameter, "Content Picker", "contentpicker", ValueType = ValueTypes.String, Group = "Pickers")]
    public class ContentPickerPropertyEditor : DataEditor
    {
        public ContentPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor();
        }
    }
}
