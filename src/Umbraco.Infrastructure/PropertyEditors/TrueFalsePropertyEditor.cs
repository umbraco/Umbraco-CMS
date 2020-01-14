using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a checkbox property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Boolean,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Checkbox",
        "boolean",
        ValueType = ValueTypes.Integer,
        Group = Constants.PropertyEditors.Groups.Common,
        Icon = "icon-checkbox")]
    public class TrueFalsePropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrueFalsePropertyEditor"/> class.
        /// </summary>
        public TrueFalsePropertyEditor(ILogger logger, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper, IShortStringHelper shortStringHelper, ILocalizedTextService localizedTextService)
            : base(logger, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new TrueFalseConfigurationEditor(_ioHelper);

    }
}
