using Umbraco.Web.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "propertyTypePickerMultiple",
        EditorType.MacroParameter,
        "Multiple Property Type Picker",
        "entitypicker")]
    public class MultiplePropertyTypeParameterEditor : DataEditor
    {
        public MultiplePropertyTypeParameterEditor(ILogger logger,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
            : base(logger, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", "1");
            DefaultConfiguration.Add("entityType", "PropertyType");
            //don't publish the id for a property type, publish its alias
            DefaultConfiguration.Add("publishBy", "alias");
        }
    }
}
