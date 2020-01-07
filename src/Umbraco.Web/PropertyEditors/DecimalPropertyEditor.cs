using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a decimal property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Decimal,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Decimal",
        "decimal",
        ValueType = ValueTypes.Decimal)]
    public class DecimalPropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalPropertyEditor"/> class.
        /// </summary>
        public DecimalPropertyEditor(ILogger logger, IShortStringHelper shortStringHelper)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService,Current.Services.TextService, shortStringHelper)
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
