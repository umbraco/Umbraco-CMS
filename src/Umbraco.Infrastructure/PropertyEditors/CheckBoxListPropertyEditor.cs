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
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IIOHelper _ioHelper;
        private readonly ILocalizedTextService _localizedTextService;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public CheckBoxListPropertyEditor(ILoggerFactory loggerFactory, ILocalizedTextService textService, IDataTypeService dataTypeService, ILocalizationService localizationService, IShortStringHelper shortStringHelper, IIOHelper ioHelper, ILocalizedTextService localizedTextService, IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService,localizedTextService, shortStringHelper, jsonSerializer)
        {
            _textService = textService;
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _shortStringHelper = shortStringHelper;
            _ioHelper = ioHelper;
            _localizedTextService = localizedTextService;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new ValueListConfigurationEditor(_textService, _ioHelper);

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new MultipleValueEditor(LoggerFactory.CreateLogger<MultipleValueEditor>(), _dataTypeService, _localizationService, _localizedTextService, _shortStringHelper, JsonSerializer, Attribute);
    }
}
