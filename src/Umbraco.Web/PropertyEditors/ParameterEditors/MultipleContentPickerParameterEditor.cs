using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor(Constants.PropertyEditors.MultiNodeTreePickerAlias, "Multiple Content Picker", "contentpicker")]
    public class MultipleContentPickerParameterEditor : ParameterEditor
    {
        public MultipleContentPickerParameterEditor()
        {
            Configuration.Add("multiPicker", "1");
            Configuration.Add("minNumber",0 );
            Configuration.Add("maxNumber", 0);
        }
    }
}
