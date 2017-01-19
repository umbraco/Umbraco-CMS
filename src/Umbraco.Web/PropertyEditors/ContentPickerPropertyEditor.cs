using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "Content Picker", PropertyEditorValueTypes.Integer, "contentpicker", IsParameterEditor = true, Group = "Pickers")]
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
            [PreValueField("showOpenButton", "Show open button (this feature is in preview!)", "boolean", Description = " Opens the node in a dialog")]
            public string ShowOpenButton { get; set; }
            
            [PreValueField("startNodeId", "Start node", "treepicker")]
            public int StartNodeId { get; set; }

        }
    }
}