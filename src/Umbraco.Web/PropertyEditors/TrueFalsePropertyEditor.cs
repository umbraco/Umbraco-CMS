using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "True/False", PropertyEditorValueTypes.Integer, "boolean", IsParameterEditor = true, Group = "Common", Icon="icon-checkbox")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public TrueFalsePropertyEditor(ILogger logger) : base(logger)
        {
        }
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TrueFalsePreValueEditor();
        }

        internal class TrueFalsePreValueEditor : PreValueEditor
        {
            [PreValueField("default", "Default Value", "boolean")]
            public string Default { get; set; }
        }
    }
}