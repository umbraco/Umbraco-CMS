using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor("propertyTypePicker", EditorType.MacroParameter, "Property Type Picker", "entitypicker")]
    public class PropertyTypeParameterEditor : DataEditor
    {
        public PropertyTypeParameterEditor(ILogger logger)
            : base(logger)
        {
            // configure
            DefaultConfiguration.Add("multiple", "0");
            DefaultConfiguration.Add("entityType", "PropertyType");
            //don't publish the id for a property type, publish its alias
            DefaultConfiguration.Add("publishBy", "alias");
        }
    }
}
