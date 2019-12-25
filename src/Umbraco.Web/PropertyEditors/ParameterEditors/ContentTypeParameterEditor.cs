using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    /// <summary>
    /// Represents a content type parameter editor.
    /// </summary>
    [DataEditor(
        "contentType",
        EditorType.MacroParameter,
        "Content Type Picker",
        "entitypicker")]
    public class ContentTypeParameterEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTypeParameterEditor"/> class.
        /// </summary>
        public ContentTypeParameterEditor(ILogger logger, ILocalizationService localizationService)
            : base(logger, Current.Services.DataTypeService, localizationService,Current.Services.TextService, Current.ShortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", false);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
