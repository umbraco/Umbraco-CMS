using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("contentTypeMultiple", "Multiple Content Type Picker", "entitypicker")]
    public class MultipleContentTypeParameterEditor : ParameterEditor
    {
        public MultipleContentTypeParameterEditor()
        {
            Configuration.Add("multiple", "1");
            Configuration.Add("entityType", "DocumentType");
        }
    }
}