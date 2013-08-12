using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.NoEdit, "Label", "readonlyvalue")]
    public class LabelPropertyEditor : PropertyEditor
    {

        protected override ValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();

            return new LabelValueEditor
            {
                View = baseEditor.View
            };
        }

    }
}