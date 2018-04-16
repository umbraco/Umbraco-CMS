using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a decimal property and parameter editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.Decimal, EditorType.PropertyValue | EditorType.MacroParameter, "Decimal", "decimal", ValueType = ValueTypes.Decimal)]
    public class DecimalPropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalPropertyEditor"/> class.
        /// </summary>
        public DecimalPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new DecimalValidator());
            return editor;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new DecimalConfigurationEditor();
    }
}
