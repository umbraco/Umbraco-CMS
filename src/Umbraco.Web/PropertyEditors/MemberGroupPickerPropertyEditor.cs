using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MemberGroupPicker,
        "Member Group Picker",
        "membergrouppicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.People,
        Icon = Constants.Icons.MemberGroup)]
    public class MemberGroupPickerPropertyEditor : DataEditor
    {
         public MemberGroupPickerPropertyEditor(ILogger logger, IShortStringHelper shortStringHelper)
             : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService, Current.Services.TextService, shortStringHelper)
         { }
    }
}
