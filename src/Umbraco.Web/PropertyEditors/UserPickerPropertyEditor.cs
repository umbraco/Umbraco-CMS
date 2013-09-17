using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.UserPickerAlias, "User picker", "INT", "userpicker")]
    public class UserPickerPropertyEditor : PropertyEditor
    {

    }
}