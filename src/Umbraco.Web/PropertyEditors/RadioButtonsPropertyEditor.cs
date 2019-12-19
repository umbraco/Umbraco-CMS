using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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
        private readonly ILocalizedTextService _textService;
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RadioButtonsPropertyEditor(ILogger logger, ILocalizedTextService textService, IIOHelper ioHelper)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService,Current.Services.TextService, Current.ShortStringHelper)
        {
            _textService = textService;
            _ioHelper = ioHelper;
        }

        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        protected override IConfigurationEditor CreateConfigurationEditor() => new ValueListConfigurationEditor(_textService, _ioHelper);
    }
}
