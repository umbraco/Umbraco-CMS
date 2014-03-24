using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MultipleMediaPickerAlias, "Media Picker", "mediapicker")]
    public class MultipleMediaPickerPropertyEditor : MediaPickerPropertyEditor
    {
        public MultipleMediaPickerPropertyEditor()
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

            [PreValueField("startNodeId", "Start node", "mediapicker")]
 			public int StartNodeId { get; set; }
        }
    }
}