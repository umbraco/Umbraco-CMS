using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.DropDownListFlexible,
        "Dropdown",
        "dropdownFlexible",
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-indent")]
    public class DropDownFlexiblePropertyEditor : DataEditor
    {
        private readonly ILocalizedTextService _textService;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly IIOHelper _ioHelper;

        public DropDownFlexiblePropertyEditor(ILocalizedTextService textService, ILogger logger, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper)
            : base(logger)
        {
            _textService = textService;
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
        }

        protected override IDataValueEditor CreateValueEditor()
        {
            return new MultipleValueEditor(Logger, _dataTypeService, _localizationService, Attribute);
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new DropDownFlexibleConfigurationEditor(_textService, _ioHelper);
    }
}
