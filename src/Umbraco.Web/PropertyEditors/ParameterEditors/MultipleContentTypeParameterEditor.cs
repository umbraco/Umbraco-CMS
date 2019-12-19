using Umbraco.Web.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "contentTypeMultiple",
        EditorType.MacroParameter,
        "Multiple Content Type Picker",
        "entitypicker")]
    public class MultipleContentTypeParameterEditor : DataEditor
    {
        public MultipleContentTypeParameterEditor(ILogger logger)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService,Current.Services.TextService, Current.ShortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", true);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
