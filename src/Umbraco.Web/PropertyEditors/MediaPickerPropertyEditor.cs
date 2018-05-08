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
    [Obsolete("This editor is obsolete, use ContentPicker2PropertyEditor instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MediaPickerAlias, "(Obsolete) Media Picker", PropertyEditorValueTypes.Integer, "mediapicker", Group = "media", Icon = "icon-picture", IsDeprecated = true)]
    public class MediaPickerPropertyEditor : MediaPicker2PropertyEditor
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
            public SingleMediaPickerPreValueEditor()
            {
                Fields.Add(new PreValueField()
                {
                    Key = "startNodeId",
                    View = "mediapicker",
                    Name = "Start node",
                    Config = new Dictionary<string, object>
                    {
                        {"idType", "int"}
                    }
                });
            }
        }
    }
}
