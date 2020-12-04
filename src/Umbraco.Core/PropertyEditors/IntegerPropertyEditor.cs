using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents an integer property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Integer,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Numeric",
        "integer",
        ValueType = ValueTypes.Integer)]
    public class IntegerPropertyEditor : DataEditor
    {
        public IntegerPropertyEditor(
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            IShortStringHelper shortStringHelper,
            ILocalizedTextService localizedTextService,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService,localizedTextService, shortStringHelper, jsonSerializer)
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
