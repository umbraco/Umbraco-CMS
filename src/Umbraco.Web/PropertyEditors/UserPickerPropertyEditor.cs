using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.UserPickerAlias, "User picker", "INT", "entitypicker")]
    public class UserPickerPropertyEditor : PropertyEditor
    {
        private IDictionary<string, object> _defaultPreValues;

        public UserPickerPropertyEditor()
        {
            _defaultPreValues = new Dictionary<string, object>
                {
                    {"entityType", "User"}
                };
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreValues; }
            set { _defaultPreValues = value; }
        }
    }
}