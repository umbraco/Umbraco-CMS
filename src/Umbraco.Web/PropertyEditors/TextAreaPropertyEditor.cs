using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TextboxMultipleAlias, "Textarea", "textarea", IsParameterEditor = true, ValueType = PropertyEditorValueTypes.Text, Icon="icon-application-window-alt")]
    public class TextAreaPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public TextAreaPropertyEditor(ILogger logger) : base(logger)
        {
        }
    }
}
