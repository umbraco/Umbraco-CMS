using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MemberPicker,
        "Member Picker",
        "memberpicker",
        ValueType = ValueTypes.String,
        Group = Constants.PropertyEditors.Groups.People,
        Icon = Constants.Icons.Member)]
    public class MemberPickerPropertyEditor : DataEditor
    {
        public MemberPickerPropertyEditor(ILogger logger)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService, Current.Services.TextService,Current.ShortStringHelper)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MemberPickerConfiguration();
    }
}
