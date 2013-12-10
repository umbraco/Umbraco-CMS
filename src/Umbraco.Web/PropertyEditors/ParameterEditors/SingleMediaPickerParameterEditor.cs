using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor(Constants.PropertyEditors.MediaPickerAlias, "Single Media Picker", "mediapicker")]
    public class SingleMediaPickerParameterEditor : ParameterEditor
    {
        public SingleMediaPickerParameterEditor()
        {
            Configuration.Add("multiPicker", "0");
        }
    }
}