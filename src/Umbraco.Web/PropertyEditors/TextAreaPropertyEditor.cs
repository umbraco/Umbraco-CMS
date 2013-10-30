using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TextboxMultipleAlias, "Textarea", "textarea", IsParameterEditor = true)]
    public class TextAreaPropertyEditor : PropertyEditor
    {
    }
}