using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "contentTypeMultiple",
        EditorType.MacroParameter,
        "Multiple Content Type Picker",
        "entitypicker")]
    public class MultipleContentTypeParameterEditor : DataEditor
    {
        public MultipleContentTypeParameterEditor(
            ILogger logger,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
            : base(logger, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", true);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
