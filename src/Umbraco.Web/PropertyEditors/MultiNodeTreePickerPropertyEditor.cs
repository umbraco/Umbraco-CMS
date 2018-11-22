using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use MultiNodeTreePickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MultiNodeTreePickerAlias, "(Obsolete) Multinode Treepicker", "contentpicker", Group = "pickers", Icon = "icon-page-add", IsDeprecated = true)]
    public class MultiNodeTreePickerPropertyEditor : MultiNodeTreePicker2PropertyEditor
    {
        public MultiNodeTreePickerPropertyEditor()
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
            preValEditor.Fields.Single(x => x.Key == "startNode").Config["idType"] = "int";
            return preValEditor;
        }
    }
}
