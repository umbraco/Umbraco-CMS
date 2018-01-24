using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.MemberPicker2, "Member Picker", "memberpicker", ValueTypes.String, Group = "People", Icon = "icon-user")]
    public class MemberPicker2PropertyEditor : PropertyEditor
    {
        public MemberPicker2PropertyEditor(ILogger logger)
            : base(logger)
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"idType", "udi"}
            };
        }

        internal IDictionary<string, object> InternalPreValues;

        public override IDictionary<string, object> DefaultPreValues
        {
            get => InternalPreValues;
            set => InternalPreValues = value;
        }
    }
}
