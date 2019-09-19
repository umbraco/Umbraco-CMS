using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use MultipleMediaPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MultipleMediaPickerAlias, "(Obsolete) Media Picker", "mediapicker", Group = "media", Icon = "icon-pictures-alt-2", IsDeprecated = true)]
    public class MultipleMediaPickerPropertyEditor : MediaPicker2PropertyEditor
    {
        public MultipleMediaPickerPropertyEditor()
        {
            //default it to multi picker
            InternalPreValues["multiPicker"] = "1";
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