using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a boolean editor.
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.Aliases.Boolean, "True/False", "boolean", ValueTypes.Integer, IsMacroParameterEditor = true, Group = "Common", Icon="icon-checkbox")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrueFalsePropertyEditor"/> class.
        /// </summary>
        public TrueFalsePropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new TrueFalseConfigurationEditor();
        }
    }
}
