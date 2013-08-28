using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;


namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MediaPicker, "Media Picker", "mediapicker")]
    public class MediaPickerPropertyEditor : PropertyEditor
    {
        protected override ValueEditor CreateValueEditor()
        {
            //TODO: Need to add some validation to the ValueEditor to ensure that any media chosen actually exists!

            return base.CreateValueEditor();
        }

    }
}
