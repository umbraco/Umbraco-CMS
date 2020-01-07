using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.UserPicker,
        "User picker",
        "entitypicker",
        ValueType = ValueTypes.Integer,
        Group = Constants.PropertyEditors.Groups.People,
        Icon = Constants.Icons.User)]
    public class UserPickerPropertyEditor : DataEditor
    {
        public UserPickerPropertyEditor(
            ILogger logger,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
            : base(logger, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new UserPickerConfiguration();
    }
}
