using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.MemberPicker2, "Member Picker", "memberpicker", ValueType = ValueTypes.String, Group = "People", Icon = "icon-user")]
    public class MemberPicker2PropertyEditor : DataEditor
    {
        public MemberPicker2PropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MemberPickerConfiguration();
    }
}
