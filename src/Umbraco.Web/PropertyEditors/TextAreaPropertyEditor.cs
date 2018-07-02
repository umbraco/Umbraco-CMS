using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a textarea property and parameter editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.TextArea, EditorType.PropertyValue | EditorType.MacroParameter, "Textarea", "textarea", ValueType = ValueTypes.Text, Icon="icon-application-window-alt")]
    public class TextAreaPropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextAreaPropertyEditor"/> class.
        /// </summary>
        public TextAreaPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new TextOnlyValueEditor(Attribute);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new TextAreaConfigurationEditor();
    }
}
