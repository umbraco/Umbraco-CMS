using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a textarea property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.TextArea,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Textarea",
        "textarea",
        ValueType = ValueTypes.Text,
        Icon = "icon-application-window-alt")]
    public class TextAreaPropertyEditor : DataEditor
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAreaPropertyEditor"/> class.
        /// </summary>
        public TextAreaPropertyEditor(ILogger logger, IDataTypeService dataTypeService, ILocalizationService localizationService)
            : base(logger)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new TextOnlyValueEditor(_dataTypeService, _localizationService, Attribute);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new TextAreaConfigurationEditor();
    }
}
