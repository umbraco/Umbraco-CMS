using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
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
         public MemberGroupPickerPropertyEditor(
             ILogger logger,
             IDataTypeService dataTypeService,
             ILocalizationService localizationService,
             ILocalizedTextService localizedTextService,
             IShortStringHelper shortStringHelper)
             : base(logger, dataTypeService, localizationService, localizedTextService, shortStringHelper)
         { }
    }
}
