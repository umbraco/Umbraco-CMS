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
    /// A property editor to allow the individual selection of pre-defined items.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.RadioButtonList,
        "Radio button list",
        "radiobuttons",
        ValueType = ValueTypes.String,
        Group = Constants.PropertyEditors.Groups.Lists,
        Icon = "icon-target")]
    public class RadioButtonsPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RadioButtonsPropertyEditor(
            ILoggerFactory loggerFactory,
            IIOHelper ioHelper,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService,localizedTextService, shortStringHelper, jsonSerializer)
        {
            _ioHelper = ioHelper;
        }

        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        protected override IConfigurationEditor CreateConfigurationEditor() => new ValueListConfigurationEditor(LocalizedTextService, _ioHelper);
    }
}
