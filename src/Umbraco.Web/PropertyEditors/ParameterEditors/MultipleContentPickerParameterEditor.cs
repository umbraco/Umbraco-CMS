using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [DataEditor("multiNodeTreePicker", EditorType.MacroParameter, "Multiple Content Picker", "contentpicker")]
    public class MultipleContentPickerParameterEditor : DataEditor
    {

        public MultipleContentPickerParameterEditor(ILogger logger, EditorType type = EditorType.PropertyValue)
            : base(logger, type)
        {
            // configure
            DefaultConfiguration.Add("multiPicker", "1");
            DefaultConfiguration.Add("minNumber",0 );
            DefaultConfiguration.Add("maxNumber", 0);
        }
    }
}
