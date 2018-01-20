using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.TextboxMultiple, "Textarea", "textarea", IsParameterEditor = true, ValueType = PropertyEditorValueTypes.Text, Icon="icon-application-window-alt")]
    public class TextAreaPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public TextAreaPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new TextOnlyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreateConfigurationEditor()
        {
            return new TextAreaPreValueEditor();
        }

        internal class TextAreaPreValueEditor : PreValueEditor
        {
            [DataTypeConfigurationField("maxChars", "Maximum allowed characters", "number", Description = "If empty - no character limit")]
            public bool MaxChars { get; set; }
        }
    }
}
