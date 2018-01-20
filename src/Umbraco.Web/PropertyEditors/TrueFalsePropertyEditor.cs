using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.Boolean, "True/False", PropertyEditorValueTypes.Integer, "boolean", IsParameterEditor = true, Group = "Common", Icon="icon-checkbox")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public TrueFalsePropertyEditor(ILogger logger) : base(logger)
        {
        }
        protected override PreValueEditor CreateConfigurationEditor()
        {
            return new TrueFalsePreValueEditor();
        }

        internal class TrueFalsePreValueEditor : PreValueEditor
        {
            [DataTypeConfigurationField("default", "Default Value", "boolean")]
            public string Default { get; set; }
        }
    }
}
