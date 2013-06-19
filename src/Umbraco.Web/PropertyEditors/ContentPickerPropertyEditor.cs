using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ContentPicker, "Content Picker", "contentpicker")]
    public class ContentPickerPropertyEditor : PropertyEditor
    {
    }
}