using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TextboxAlias, "Textbox", "textbox", IsParameterEditor = true, Group = "Common")]
    public class TextboxPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public TextboxPropertyEditor(ILogger logger)
            : base(logger)
        { }
        
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
            [PreValueField("maxChars", "Maximum allowed characters", "number", Description = "If empty - no character limit")]
            public bool MaxChars { get; set; }
        }
    }
}
