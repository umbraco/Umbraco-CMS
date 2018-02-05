using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [ValueEditor(Constants.PropertyEditors.Aliases.ContentPicker2Alias, "Content Picker", "contentpicker", ValueTypes.String, IsMacroParameterEditor = true, Group = "Pickers")]
    public class ContentPicker2PropertyEditor : PropertyEditor
    {
        public ContentPicker2PropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new ContentPickerConfigurationEditor();
        }
    }
}
