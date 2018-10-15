using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TextboxMultipleAlias, "Textarea", "textarea", IsParameterEditor = true, ValueType = PropertyEditorValueTypes.Text, Icon="icon-application-window-alt")]
    public class TextAreaPropertyEditor : PropertyEditor
    {
        protected override PropertyValueEditor CreateValueEditor()
        {
            return new TextOnlyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TextAreaPreValueEditor();
        }

        internal class TextAreaPreValueEditor : PreValueEditor
        {
            [PreValueField("maxChars", "Maximum allowed characters", "number", Description = "If empty - no character limit")]
            public int MaxChars { get; set; }

            [PreValueField("rows", "Number of rows", "number", Description = "If empty - 10 rows would be set as the default value")]
            public int Rows { get; set; }
        }
    }
}
