using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

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
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IIOHelper _ioHelper;

        public DropDownFlexiblePropertyEditor(
            ILocalizedTextService textService,
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            IShortStringHelper shortStringHelper,
            IIOHelper ioHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, textService, shortStringHelper, jsonSerializer)
        {
            _textService = textService;
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _shortStringHelper = shortStringHelper;
            _ioHelper = ioHelper;
        }

        protected override IDataValueEditor CreateValueEditor()
        {
            return new MultipleValueEditor(LoggerFactory.CreateLogger<MultipleValueEditor>(), _dataTypeService, _localizationService, _textService, _shortStringHelper, JsonSerializer, Attribute);
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new DropDownFlexibleConfigurationEditor(_textService, _ioHelper);
    }
}
