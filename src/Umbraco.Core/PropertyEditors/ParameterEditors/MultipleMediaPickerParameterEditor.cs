using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
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
        public MultipleMediaPickerParameterEditor(
            IDataValueEditorFactory dataValueEditorFactory)
            : base(dataValueEditorFactory)
        {
            DefaultConfiguration.Add("multiPicker", "1");
        }
    }
}
