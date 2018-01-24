using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a textarea editor.
    /// </summary>
    [ValueEditor(Constants.PropertyEditors.Aliases.TextboxMultiple, "Textarea", "textarea", IsMacroParameterEditor = true, ValueType = ValueTypes.Text, Icon="icon-application-window-alt")]
    public class TextAreaPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextAreaPropertyEditor"/> class.
        /// </summary>
        public TextAreaPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override ValueEditor CreateValueEditor() => new TextOnlyValueEditor(Attribute);

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor() => new TextAreaConfigurationEditor();
    }
}
