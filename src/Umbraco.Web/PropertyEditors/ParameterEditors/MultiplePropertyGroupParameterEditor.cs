using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("tabPickerMultiple", "Multiple Tab Picker", "entitypicker")]
    public class MultiplePropertyGroupParameterEditor : ParameterEditor
    {
        public MultiplePropertyGroupParameterEditor()
        {
            Configuration.Add("multiple", "1");
            Configuration.Add("entityType", "PropertyGroup");
            //don't publish the id for a property group, publish it's alias (which is actually just it's lower cased name)
            Configuration.Add("publishBy", "alias");
        }
    }
}