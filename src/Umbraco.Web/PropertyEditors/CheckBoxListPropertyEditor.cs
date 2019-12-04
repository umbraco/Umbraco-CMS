using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A property editor to allow multiple checkbox selection of pre-defined items.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.CheckBoxList,
        "Checkbox list",
        "checkboxlist",
        Icon = "icon-bulleted-list",
        Group = Constants.PropertyEditors.Groups.Lists)]
    public class CheckBoxListPropertyEditor : DataEditor
    {
        private readonly ILocalizedTextService _textService;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public CheckBoxListPropertyEditor(ILogger logger, ILocalizedTextService textService, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper)
            : base(logger)
        {
            _textService = textService;
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ValueListConfigurationEditor(_textService, _ioHelper);

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new MultipleValueEditor(Logger, _dataTypeService, _localizationService, Attribute);
    }
}
