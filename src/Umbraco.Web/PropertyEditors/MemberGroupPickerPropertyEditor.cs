using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
     [PropertyEditor(Constants.PropertyEditors.MemberGroupPickerAlias, "Member Group Picker", "membergrouppicker")]
    public class MemberGroupPickerPropertyEditor : PropertyEditor
    {
    }
}
