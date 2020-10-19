using Microsoft.Extensions.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

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
        public ContentTypeParameterEditor(
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            // configure
            DefaultConfiguration.Add("multiple", false);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
