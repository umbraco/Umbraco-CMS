using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.NoEditAlias, "Label", "readonlyvalue")]
    public class LabelPropertyEditor : PropertyEditor
    {

        protected override ValueEditor CreateValueEditor()
        {
            return new LabelValueEditor(base.CreateValueEditor());
        }

    }
}