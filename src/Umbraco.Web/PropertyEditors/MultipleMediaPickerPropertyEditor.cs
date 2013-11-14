using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MultipleMediaPickerAlias, "Multiple Media Picker", "mediapicker")]
    public class MultipleMediaPickerPropertyEditor : MediaPickerPropertyEditor
    {
        public MultipleMediaPickerPropertyEditor()
        {
            //clear the pre-values so it defaults to a multiple picker.
            InternalPreValues.Clear();
        }
    }
}