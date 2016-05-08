using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TextboxMultipleAlias, "Textarea", "textarea", IsParameterEditor = true, ValueType = PropertyEditorValueTypes.TextType, Icon="icon-application-window-alt")]
    public class TextAreaPropertyEditor : PropertyEditor
    {
    }
}
