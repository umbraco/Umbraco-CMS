using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MemberPickerAlias, "Member Picker", "INT", "memberpicker", Group = "People", Icon = "icon-user")]
    public class MemberPickerPropertyEditor : PropertyEditor
    {
    }
}
