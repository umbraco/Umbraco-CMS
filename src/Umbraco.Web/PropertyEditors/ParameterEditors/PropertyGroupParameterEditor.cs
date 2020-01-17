using Umbraco.Web.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "tabPicker",
        EditorType.MacroParameter,
        "Tab Picker",
        "entitypicker")]
    public class PropertyGroupParameterEditor : DataEditor
    {
        public PropertyGroupParameterEditor(
            ILogger logger,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
            : base(logger, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", "0");
            DefaultConfiguration.Add("entityType", "PropertyGroup");
            //don't publish the id for a property group, publish it's alias (which is actually just it's lower cased name)
            DefaultConfiguration.Add("publishBy", "alias");
        }
    }
}
