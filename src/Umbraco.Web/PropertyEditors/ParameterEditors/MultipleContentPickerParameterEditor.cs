using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    /// <summary>
    /// Represents a parameter editor of some sort.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, EditorType.MacroParameter, "Multiple Content Picker", "contentpicker")]
    public class MultipleContentPickerParameterEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleContentPickerParameterEditor"/> class.
        /// </summary>
        public MultipleContentPickerParameterEditor(ILogger logger)
            : base(logger)
        {
            // configure
            DefaultConfiguration.Add("multiPicker", "1");
            DefaultConfiguration.Add("minNumber",0 );
            DefaultConfiguration.Add("maxNumber", 0);
        }
    }
}
