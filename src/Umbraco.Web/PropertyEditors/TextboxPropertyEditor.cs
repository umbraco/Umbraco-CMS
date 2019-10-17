using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TextboxAlias, "Textbox", "textbox", IsParameterEditor = true, Group = "Common")]
    public class TextboxPropertyEditor : PropertyEditor
    {
        public TextboxPropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    {"maxChars", null}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new TextOnlyValueEditor(base.CreateValueEditor());
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TextboxPreValueEditor();
        }

        internal class TextboxPreValueEditor : PreValueEditor
        {
            [PreValueField("maxChars", "Maximum allowed characters", "textstringlimited", Description = "If empty - 500 character limit")]
            public bool MaxChars { get; set; }
        }
    }
}
