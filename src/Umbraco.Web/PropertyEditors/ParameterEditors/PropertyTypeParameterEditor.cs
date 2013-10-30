using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("propertyTypePicker", "Property Type Picker", "entitypicker")]
    public class PropertyTypeParameterEditor : ParameterEditor
    {
        public PropertyTypeParameterEditor()
        {
            Configuration.Add("multiple", "0");
            Configuration.Add("entityType", "PropertyType");
            //don't publish the id for a property type, publish it's alias
            Configuration.Add("publishBy", "alias");
        }
    }
}