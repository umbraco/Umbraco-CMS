using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

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
        private readonly IIOHelper _ioHelper;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IShortStringHelper _shortStringHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAreaPropertyEditor"/> class.
        /// </summary>
        public TextAreaPropertyEditor(
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            IIOHelper ioHelper,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
            _localizedTextService = localizedTextService;
            _shortStringHelper = shortStringHelper;
        }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new TextOnlyValueEditor(_dataTypeService, _localizationService, Attribute, _localizedTextService, _shortStringHelper, JsonSerializer);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new TextAreaConfigurationEditor(_ioHelper);
    }
}
