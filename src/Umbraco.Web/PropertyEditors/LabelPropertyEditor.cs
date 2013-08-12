using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.NoEdit, "Label", "readonlyvalue")]
    public class LabelPropertyEditor : PropertyEditor
    {
    }
}