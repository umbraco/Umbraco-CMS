using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.UserPicker, "User picker", "entitypicker", ValueType = ValueTypes.Integer, Group = "People", Icon = Constants.Icons.User)]
    public class UserPickerPropertyEditor : DataEditor
    {
        public UserPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new UserPickerConfiguration();
    }
}
