using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use ContentPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "(Obsolete) Content Picker", PropertyEditorValueTypes.Integer, "contentpicker", IsParameterEditor = true, Group = "Pickers", IsDeprecated = true)]
    public class ContentPickerPropertyEditor : ContentPicker2PropertyEditor
    {
        public ContentPickerPropertyEditor(ILogger logger)
            : base(logger)
        {
            InternalPreValues["idType"] = "int";
        }

        protected override PreValueEditor CreateConfigurationEditor()
        {
            var preValEditor = base.CreateConfigurationEditor();
            preValEditor.Fields.Single(x => x.Key == "startNodeId").Config["idType"] = "int";
            return preValEditor;
        }
    }
}
