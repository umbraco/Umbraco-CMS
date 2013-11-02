using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("propertyTypePickerMultiple", "Multiple Property Type Picker", "entitypicker")]
    public class MultiplePropertyTypeParameterEditor : ParameterEditor
    {
        public MultiplePropertyTypeParameterEditor()
        {
            Configuration.Add("multiple", "1");
            Configuration.Add("entityType", "PropertyType");
            //don't publish the id for a property type, publish it's alias
            Configuration.Add("publishBy", "alias");
        }
    }
}