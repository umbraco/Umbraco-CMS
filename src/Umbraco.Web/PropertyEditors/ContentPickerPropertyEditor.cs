using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "Content Picker", "INT", "contentpicker", IsParameterEditor = true)]
    public class ContentPickerPropertyEditor : PropertyEditor
    {
    }
}