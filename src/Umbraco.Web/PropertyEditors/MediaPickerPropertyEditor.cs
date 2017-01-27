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
    }

    /// <summary>
    /// Media picker property editors that stores UDI
    /// </summary>
    [PropertyEditor(Constants.PropertyEditors.MediaPicker2Alias, "Media Picker", PropertyEditorValueTypes.String, "mediapicker", Group = "media", Icon = "icon-picture")]
    public class MediaPickerPropertyEditor2 : PropertyEditor
    {
        public MediaPickerPropertyEditor2()
        {
            InternalPreValues = new Dictionary<string, object>
                {
                    {"multiPicker", "0"},
                    {"onlyImages", "0"}
                };
        }

        protected IDictionary<string, object> InternalPreValues;

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new SingleMediaPickerValueEditor();
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return InternalPreValues; }
            set { InternalPreValues = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new SingleMediaPickerPreValueEditor();
        }

        internal class SingleMediaPickerValueEditor : PropertyValueEditor
        {
            override 
        }

        internal class SingleMediaPickerPreValueEditor : PreValueEditor
        {
            [PreValueField("startNodeId", "Start node", "mediapicker")]
            public int StartNodeId { get; set; }
        }
    }
}
