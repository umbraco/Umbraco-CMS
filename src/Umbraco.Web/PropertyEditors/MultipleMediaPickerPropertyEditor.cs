using System;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use MultipleMediaPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MultipleMediaPickerAlias, "(Obsolete) Media Picker", "mediapicker", Group = "media", Icon = "icon-pictures-alt-2", IsDeprecated = true)]
    public class MultipleMediaPickerPropertyEditor : MediaPickerPropertyEditor
    {
        
    }

    [PropertyEditor(Constants.PropertyEditors.MultipleMediaPicker2Alias, "Media Picker", PropertyEditorValueTypes.Text, "mediapicker", Group = "media", Icon = "icon-pictures-alt-2", IsDeprecated = true)]
    public class MultipleMediaPickerPropertyEditor2 : MediaPickerPropertyEditor2
    {
        public MultipleMediaPickerPropertyEditor2()
        {
            //clear the pre-values so it defaults to a multiple picker.
            InternalPreValues.Clear();
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MediaPickerPreValueEditor();
        }

        internal class MediaPickerPreValueEditor : PreValueEditor
        {
            [PreValueField("multiPicker", "Pick multiple items", "boolean")]
            public bool MultiPicker { get; set; }

            [PreValueField("onlyImages", "Pick only images", "boolean", Description = "Only let the editor choose images from media.")]
            public bool OnlyImages { get; set; }

            [PreValueField("disableFolderSelect", "Disable folder select", "boolean", Description = "Do not allow folders to be picked.")]
            public bool DisableFolderSelect { get; set; }

            [PreValueField("startNodeId", "Start node", "mediapicker")]
 			public int StartNodeId { get; set; }
        }
    }
}