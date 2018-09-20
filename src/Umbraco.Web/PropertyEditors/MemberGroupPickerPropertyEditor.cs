using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.MemberGroupPicker, "Member Group Picker", "membergrouppicker", Group="People", Icon="icon-users")]
    public class MemberGroupPickerPropertyEditor : DataEditor
    {
         public MemberGroupPickerPropertyEditor(ILogger logger)
             : base(logger)
         { }
    }
}
