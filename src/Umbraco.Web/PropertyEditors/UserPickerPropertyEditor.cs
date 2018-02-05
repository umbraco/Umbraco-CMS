using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.UserPicker, "User picker", "entitypicker", ValueTypes.Integer, Group="People", Icon="icon-user")]
    public class UserPickerPropertyEditor : PropertyEditor
    {
        public UserPickerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override ConfigurationEditor CreateConfigurationEditor() => new UserPickerConfiguration();
    }

    public class UserPickerConfiguration : ConfigurationEditor
    {
        public override IDictionary<string, object> DefaultConfiguration => new Dictionary<string, object>
        {
            {"entityType", "User"}
        };
    }
}
