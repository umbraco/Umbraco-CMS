using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a textbox property and parameter editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.TextBox, EditorType.PropertyValue | EditorType.MacroParameter, "Textbox", "textbox", Group = "Common")]
    public class TextboxPropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxPropertyEditor"/> class.
        /// </summary>
        public TextboxPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc/>
        protected override IDataValueEditor CreateValueEditor() => new TextOnlyValueEditor(Attribute);

        /// <inheritdoc/>
        protected override IConfigurationEditor CreateConfigurationEditor() => new TextboxConfigurationEditor();
    }
}
