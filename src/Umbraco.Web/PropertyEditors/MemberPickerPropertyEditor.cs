using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    [Obsolete("This editor is obsolete, use MemberPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MemberPickerAlias, "(Obsolete) Member Picker", PropertyEditorValueTypes.Integer, "memberpicker", Group = "People", Icon = "icon-user", IsDeprecated = true)]
    public class MemberPickerPropertyEditor : MemberPicker2PropertyEditor
    {
        public MemberPickerPropertyEditor()
        {
            InternalPreValues["idType"] = "int";
        }
    }
}
