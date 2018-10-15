using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MemberPicker2Alias, "Member Picker", PropertyEditorValueTypes.String, "memberpicker", Group = "People", Icon = "icon-user")]
    public class MemberPicker2PropertyEditor : PropertyEditor
    {
        public MemberPicker2PropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"idType", "udi"}
            };
        }

        internal IDictionary<string, object> InternalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return InternalPreValues; }
            set { InternalPreValues = value; }
        }
    }
}