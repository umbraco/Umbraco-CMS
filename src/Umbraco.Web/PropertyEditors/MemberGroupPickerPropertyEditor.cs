using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.MemberGroupPicker, "Member Group Picker", "membergrouppicker", ValueType = ValueTypes.Text, Group = "People", Icon = Constants.Icons.MemberGroup)]
    public class MemberGroupPickerPropertyEditor : DataEditor
    {
         public MemberGroupPickerPropertyEditor(ILogger logger)
             : base(logger)
         { }
    }
}
