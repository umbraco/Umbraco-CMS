using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    /// <summary>
    /// Represents a content type parameter editor.
    /// </summary>
    [DataEditor("contentType", EditorType.MacroParameter, "Content Type Picker", "entitypicker")]
    public class ContentTypeParameterEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTypeParameterEditor"/> class.
        /// </summary>
        public ContentTypeParameterEditor(ILogger logger)
            : base(logger)
        {
            // configure
            DefaultConfiguration.Add("multiple", false);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
