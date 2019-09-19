using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    /// <summary>
    /// Legacy content property editor that stores Integer Ids
    /// </summary>
    [Obsolete("This editor is obsolete, use ContentPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "(Obsolete) Content Picker", PropertyEditorValueTypes.Integer, "contentpicker", IsParameterEditor = true, Group = "Pickers", IsDeprecated = true)]
    public class ContentPickerPropertyEditor : ContentPicker2PropertyEditor
    {
        public ContentPickerPropertyEditor()
        {
            InternalPreValues["idType"] = "int";
        }

        /// <summary>
        /// overridden to change the pre-value picker to use INT ids
        /// </summary>
        /// <returns></returns>
        protected override PreValueEditor CreatePreValueEditor()
        {
            var preValEditor = base.CreatePreValueEditor();
            preValEditor.Fields.Single(x => x.Key == "startNodeId").Config["idType"] = "int";
            return preValEditor;
        }
    }
}