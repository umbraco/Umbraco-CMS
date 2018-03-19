using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.MemberPicker, "Member Picker", "memberpicker", ValueType = ValueTypes.String, Group = "People", Icon = "icon-user")]
    public class MemberPickerPropertyEditor : DataEditor
    {
        public MemberPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MemberPickerConfiguration();
    }
}
