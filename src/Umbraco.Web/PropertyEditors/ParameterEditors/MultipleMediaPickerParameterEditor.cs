using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor(Constants.PropertyEditors.MultipleMediaPickerAlias, "Multiple Media Picker", "mediapicker")]
    public class MultipleMediaPickerParameterEditor : ParameterEditor
    {
        public MultipleMediaPickerParameterEditor()
        {
            Configuration.Add("multiPicker", "1");
        }
    }
}