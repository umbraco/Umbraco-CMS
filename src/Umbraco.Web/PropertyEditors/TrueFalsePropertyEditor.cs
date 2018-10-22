using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a checkbox property and parameter editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.Boolean, EditorType.PropertyValue | EditorType.MacroParameter, "Checkbox", "boolean", ValueType = ValueTypes.Integer, Group = "Common", Icon="icon-checkbox")]
    public class TrueFalsePropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrueFalsePropertyEditor"/> class.
        /// </summary>
        public TrueFalsePropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new TrueFalseConfigurationEditor();

    }
}
