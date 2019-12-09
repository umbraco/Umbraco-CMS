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

        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxPropertyEditor"/> class.
        /// </summary>
        public TextboxPropertyEditor(ILogger logger, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper, IShortStringHelper shortStringHelper)
            : base(logger, dataTypeService, localizationService, shortStringHelper)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
        }

        /// <inheritdoc/>
        protected override IDataValueEditor CreateValueEditor() => new TextOnlyValueEditor(_dataTypeService, _localizationService, Attribute);

        /// <inheritdoc/>
        protected override IConfigurationEditor CreateConfigurationEditor() => new TextboxConfigurationEditor(_ioHelper);
    }
}
