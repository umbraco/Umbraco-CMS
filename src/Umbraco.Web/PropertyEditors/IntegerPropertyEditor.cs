using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents an integer property and parameter editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.Integer, EditorType.PropertyValue | EditorType.MacroParameter, "Numeric", "integer", ValueType = ValueTypes.Integer)]
    public class IntegerPropertyEditor : DataEditor
    {
        public IntegerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new IntegerValidator()); // ensure the value is validated
            return editor;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new IntegerConfigurationEditor();
    }
}
