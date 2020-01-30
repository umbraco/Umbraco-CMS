using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    /// <summary>
    /// Represents a multiple media picker macro parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultipleMediaPicker,
        EditorType.MacroParameter,
        "Multiple Media Picker",
        "mediapicker",
        ValueType = ValueTypes.Text)]
    public class MultipleMediaPickerParameterEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleMediaPickerParameterEditor"/> class.
        /// </summary>
        public MultipleMediaPickerParameterEditor(ILogger logger)
            : base(logger)
        {
            DefaultConfiguration.Add("multiPicker", "1");
        }
    }
}
