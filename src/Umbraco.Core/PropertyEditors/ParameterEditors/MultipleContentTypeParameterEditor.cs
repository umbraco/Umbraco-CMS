using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
{
    [DataEditor(
        "contentTypeMultiple",
        EditorType.MacroParameter,
        "Multiple Content Type Picker",
        "entitypicker")]
    public class MultipleContentTypeParameterEditor : DataEditor
    {
        public MultipleContentTypeParameterEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
            // configure
            DefaultConfiguration.Add("multiple", true);
            DefaultConfiguration.Add("entityType", "DocumentType");
        }
    }
}
