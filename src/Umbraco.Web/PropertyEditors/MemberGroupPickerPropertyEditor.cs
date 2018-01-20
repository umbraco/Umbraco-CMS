using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
     [PropertyEditor(Constants.PropertyEditors.Aliases.MemberGroupPicker, "Member Group Picker", "membergrouppicker", Group="People", Icon="icon-users")]
    public class MemberGroupPickerPropertyEditor : PropertyEditor
    {
         /// <summary>
         /// The constructor will setup the property editor based on the attribute if one is found
         /// </summary>
         public MemberGroupPickerPropertyEditor(ILogger logger) : base(logger)
         {
         }
    }
}
