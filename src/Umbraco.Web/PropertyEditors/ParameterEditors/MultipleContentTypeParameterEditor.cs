using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "contentTypeMultiple",
        EditorType.MacroParameter,
        "Multiple Content Type Picker",
        "entitypicker")]
    public class MultipleContentTypeParameterEditor : DataEditor
    {
        public MultipleContentTypeParameterEditor(ILogger logger, ILocalizationService localizationService)
            : base(logger, Current.Services.DataTypeService, localizationService,Current.Services.TextService, Current.ShortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", true);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
