using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a textbox editor.
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.Aliases.Textbox, "Textbox", "textbox", IsMacroParameterEditor = true, Group = "Common")]
    public class TextboxPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxPropertyEditor"/> class.
        /// </summary>
        public TextboxPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc/>
        protected override ValueEditor CreateValueEditor()
        {
            return new TextOnlyValueEditor(base.CreateValueEditor());
        }

        /// <inheritdoc/>
        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new TextboxConfigurationEditor();
        }
    }
}
