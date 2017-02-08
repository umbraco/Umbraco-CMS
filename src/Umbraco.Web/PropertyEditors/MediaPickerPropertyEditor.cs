using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;


namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Legacy media property editor that stores Integer Ids
    /// </summary>
    [Obsolete("This editor is obsolete, use ContentPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MediaPickerAlias, "(Obsolete) Media Picker", PropertyEditorValueTypes.Integer, "mediapicker", Group = "media", Icon = "icon-picture", IsDeprecated = true)]
    public class MediaPickerPropertyEditor : MediaPickerPropertyEditor2
    {
        public MediaPickerPropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
                {
                    {"multiPicker", "0"},
                    {"onlyImages", "0"},
                    {"idType", "int"}
                };
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new SingleMediaPickerPreValueEditor();
        }

        internal class SingleMediaPickerPreValueEditor : PreValueEditor
        {
            [PreValueField("startNodeId", "Start node", "mediapicker")]
            public int StartNodeId { get; set; }
        }
    }

    /// <summary>
    /// Media picker property editors that stores UDI
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.MediaPicker2Alias, "Media Picker", PropertyEditorValueTypes.Text, "mediapicker", Group = "media", Icon = "icon-picture")]
    public class MediaPickerPropertyEditor2 : PropertyEditor
    {
        public MediaPickerPropertyEditor2()
        {
            InternalPreValues = new Dictionary<string, object>
                {
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
            return new MediaPickerPreValueEditor();
        }

        internal class MediaPickerPreValueEditor : PreValueEditor
        {
            public MediaPickerPreValueEditor()
            {
                //create the fields
                Fields.Add(new PreValueField()
                {
                    Key = "multiPicker",
                    View = "boolean",
                    Name = "Pick multiple items"
                });
                Fields.Add(new PreValueField()
                {
                    Key = "onlyImages",
                    View = "boolean",
                    Name = "Pick only images",
                    Description = "Only let the editor choose images from media."
                });
                Fields.Add(new PreValueField()
                {
                    Key = "disableFolderSelect",
                    View = "boolean",
                    Name = "Disable folder select",
                    Description = "Do not allow folders to be picked."
                });
                Fields.Add(new PreValueField()
                {
                    Key = "startNodeId",
                    View = "mediapicker",
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
