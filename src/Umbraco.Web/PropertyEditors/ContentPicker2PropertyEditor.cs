using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.ContentPicker2Alias, EditorType.PropertyValue | EditorType.MacroParameter, "Content Picker", "contentpicker", ValueType = ValueTypes.String, Group = "Pickers")]
    public class ContentPicker2PropertyEditor : DataEditor
    {
        public ContentPicker2PropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor();
        }
    }
}
