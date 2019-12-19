using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "propertyTypePickerMultiple",
        EditorType.MacroParameter,
        "Multiple Property Type Picker",
        "entitypicker")]
    public class MultiplePropertyTypeParameterEditor : DataEditor
    {
        public MultiplePropertyTypeParameterEditor(ILogger logger)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService, Current.Services.TextService,Current.ShortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", "1");
            DefaultConfiguration.Add("entityType", "PropertyType");
            //don't publish the id for a property type, publish its alias
            DefaultConfiguration.Add("publishBy", "alias");
        }
    }
}
