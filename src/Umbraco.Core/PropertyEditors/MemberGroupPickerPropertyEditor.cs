using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
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
             IDataValueEditorFactory dataValueEditorFactory)
             : base(dataValueEditorFactory)
         { }
    }
}
