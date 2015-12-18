using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "Content Picker", "INT", "contentpicker", IsParameterEditor = true)]
    public class ContentPickerPropertyEditor : PropertyEditor
    {

        public ContentPickerPropertyEditor()
        {
            _internalPreValues = new Dictionary<string, object>
            {
                {"startNodeId", "-1"},
                {"showOpenButton", "0"},
                {"showEditButton", "0"},
                {"showPathOnHover", "0"}
            };
        }

        private IDictionary<string, object> _internalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _internalPreValues; }
            set { _internalPreValues = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ContentPickerPreValueEditor();
        }

        internal class ContentPickerPreValueEditor : PreValueEditor
        {
            [PreValueField("showOpenButton", "Show open button", "boolean")]
            public string ShowOpenButton { get; set; }

            [PreValueField("showEditButton", "Show edit button (this feature is in preview!)", "boolean")]
            public string ShowEditButton { get; set; }
            
            [PreValueField("startNodeId", "Start node", "treepicker")]
            public int StartNodeId { get; set; }

            [PreValueField("showPathOnHover", "Show path when hovering items", "boolean")]
            public bool ShowPathOnHover { get; set; }
        }
    }
}