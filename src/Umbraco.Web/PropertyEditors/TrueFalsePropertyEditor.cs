using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "Checkbox", PropertyEditorValueTypes.Integer, "boolean", IsParameterEditor = true, Group = "Common", Icon="icon-checkbox")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
        public TrueFalsePropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    {"default", false},
                    {"labelOn", ""}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TrueFalsePreValueEditor();
        }

        internal class TrueFalsePreValueEditor : PreValueEditor
        {
            [PreValueField("default", "Default Value", "boolean")]
            public string Default { get; set; }

            public TrueFalsePreValueEditor()
            {
                Fields.Add(new PreValueField()
                {
                    Description = "Write a label text",
                    Key = "labelOn",
                    Name = "Label",
                    View = "textstring"
                });
            }
        }
    }
}
