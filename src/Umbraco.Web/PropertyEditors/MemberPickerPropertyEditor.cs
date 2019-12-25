using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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
        public MemberPickerPropertyEditor(ILogger logger, ILocalizationService localizationService)
            : base(logger, Current.Services.DataTypeService, localizationService, Current.Services.TextService,Current.ShortStringHelper)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MemberPickerConfiguration();
    }
}
