using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;


namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MediaPickerAlias, "Media Picker", "INT", "mediapicker")]
    public class MediaPickerPropertyEditor : PropertyEditor
    {
        public MediaPickerPropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
                {
                    {"multiPicker", false}
                };
        }

        protected IDictionary<string, object> InternalPreValues;

        protected override PropertyValueEditor CreateValueEditor()
        {
            //TODO: Need to add some validation to the ValueEditor to ensure that any media chosen actually exists!

            return base.CreateValueEditor();
            
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return InternalPreValues; }
            set { InternalPreValues = value; }
        }
    }
}
