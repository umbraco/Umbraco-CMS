using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
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
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
            // configure
            DefaultConfiguration.Add("multiple", false);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
