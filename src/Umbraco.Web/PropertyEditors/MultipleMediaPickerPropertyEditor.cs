using System;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use MultipleMediaPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MultipleMediaPickerAlias, "(Obsolete) Media Picker", "mediapicker", Group = "media", Icon = "icon-pictures-alt-2", IsDeprecated = true)]
    public class MultipleMediaPickerPropertyEditor : MediaPickerPropertyEditor
    {
        public MultipleMediaPickerPropertyEditor()
        {
            //clear the pre-values so it defaults to a multiple picker.
            InternalPreValues.Clear();
        }
    }    
}