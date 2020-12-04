using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
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
             ILoggerFactory loggerFactory,
             IDataTypeService dataTypeService,
             ILocalizationService localizationService,
             ILocalizedTextService localizedTextService,
             IShortStringHelper shortStringHelper,
             IJsonSerializer jsonSerializer)
             : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
         { }
    }
}
