using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.Integer, "Numeric", "integer", IsMacroParameterEditor = true, ValueType = ValueTypes.Integer)]
    public class IntegerPropertyEditor : PropertyEditor
    {
        public IntegerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override ValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new IntegerValidator()); // ensure the value is validated
            return editor;
        }

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new IntegerConfigurationEditor();
        }
    }
}
