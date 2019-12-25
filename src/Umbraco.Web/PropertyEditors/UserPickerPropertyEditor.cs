using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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
        public UserPickerPropertyEditor(ILogger logger, ILocalizationService localizationService)
            : base(logger, Current.Services.DataTypeService, localizationService, Current.Services.TextService,Current.ShortStringHelper)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new UserPickerConfiguration();
    }
}
