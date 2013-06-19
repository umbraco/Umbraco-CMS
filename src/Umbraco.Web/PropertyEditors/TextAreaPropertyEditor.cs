using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TextboxMultiple, "Textarea", "textarea")]
    public class TextAreaPropertyEditor : PropertyEditor
    {
    }
}