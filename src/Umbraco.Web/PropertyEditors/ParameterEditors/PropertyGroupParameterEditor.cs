using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("tabPicker", "Tab Picker", "entitypicker")]
    public class PropertyGroupParameterEditor : ParameterEditor
    {
        public PropertyGroupParameterEditor()
        {
            Configuration.Add("multiple", "0");
            Configuration.Add("entityType", "PropertyGroup");
            //don't publish the id for a property group, publish it's alias (which is actually just it's lower cased name)
            Configuration.Add("publishBy", "alias");
        }
    }
}