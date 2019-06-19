using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.ContentPicker2Alias, "Content Picker", PropertyEditorValueTypes.String, "contentpicker", IsParameterEditor = true, Group = "Pickers")]
    public class ContentPicker2PropertyEditor : PropertyEditor
    {

        public ContentPicker2PropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"startNodeId", "-1"},                
                {"showOpenButton", "0"},
                {"showEditButton", "0"},
                {"showPathOnHover", "0"},
                {"ignoreUserStartNodes", "0"},
                {"idType", "udi"}
            };
        }

        internal IDictionary<string, object> InternalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return InternalPreValues; }
            set { InternalPreValues = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ContentPickerPreValueEditor();
        }

        internal class ContentPickerPreValueEditor : PreValueEditor
        {
            public ContentPickerPreValueEditor()
            {
                //create the fields                
                Fields.Add(new PreValueField()
                {                    
                    Key = "showOpenButton",
                    View = "boolean",
                    Name = "Show open button (this feature is in preview!)",
                    Description = "Opens the node in a dialog"
                });
                Fields.Add(new PreValueField()
                {
                    Key = "ignoreUserStartNodes",
                    View = "boolean",
                    Name = "Ignore user start nodes",
                    Description = "Selecting this option allows a user to choose nodes that they normally don't have access to."
                });
                Fields.Add(new PreValueField()
                {
                    Key = "startNodeId",
                    View = "treepicker",
                    Name = "Start node",
                    Config = new Dictionary<string, object>
                    {
                        {"idType", "udi"}
                    }
                });
            }
        }
    }
}
