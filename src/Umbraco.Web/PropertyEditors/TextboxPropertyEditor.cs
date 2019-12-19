using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a textbox property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.TextBox,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Textbox",
        "textbox",
        Group = Constants.PropertyEditors.Groups.Common)]
    public class TextboxPropertyEditor : DataEditor
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly IIOHelper _ioHelper;
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ILocalizedTextService _localizedTextService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxPropertyEditor"/> class.
        /// </summary>
        public TextboxPropertyEditor(ILogger logger, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper, IShortStringHelper shortStringHelper, ILocalizedTextService localizedTextService)
            : base(logger, dataTypeService, localizationService,localizedTextService, shortStringHelper)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
            _shortStringHelper = shortStringHelper;
            _localizedTextService = localizedTextService;
        }

        /// <inheritdoc/>
        protected override IDataValueEditor CreateValueEditor() => new TextOnlyValueEditor(_dataTypeService, _localizationService, Attribute, _localizedTextService, _shortStringHelper);

        /// <inheritdoc/>
        protected override IConfigurationEditor CreateConfigurationEditor() => new TextboxConfigurationEditor(_ioHelper);
    }
}
