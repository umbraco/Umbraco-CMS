using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    /// <summary>
    /// Legacy content property editor that stores Integer Ids
    /// </summary>
    [Obsolete("This editor is obsolete, use ContentPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.ContentPickerAlias, "(Obsolete) Content Picker", PropertyEditorValueTypes.Integer, "contentpicker", IsParameterEditor = true, Group = "Pickers", IsDeprecated = true)]
    public class ContentPickerPropertyEditor : ContentPickerPropertyEditor2
    {
        public ContentPickerPropertyEditor()
        {
            InternalPreValues["idType"] = "int";
        }

        /// <summary>
        /// overridden to change the pre-value picker to use INT ids
        /// </summary>
        /// <returns></returns>
        protected override PreValueEditor CreatePreValueEditor()
        {
            var preValEditor = base.CreatePreValueEditor();
            preValEditor.Fields.Single(x => x.Key == "startNodeId").Config["idType"] = "int";
            return preValEditor;
        }
    }

    /// <summary>
    /// Content property editor that stores UDI
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.ContentPicker2Alias, "Content Picker", PropertyEditorValueTypes.String, "contentpicker", IsParameterEditor = true, Group = "Pickers")]
    public class ContentPickerPropertyEditor2 : PropertyEditor
    {

        public ContentPickerPropertyEditor2()
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"startNodeId", "-1"},
                {"showOpenButton", "0"},
                {"showEditButton", "0"},
                {"showPathOnHover", "0"},
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