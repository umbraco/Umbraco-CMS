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
            _defaultPreValues = new Dictionary<string, object>
                {
                    {"multiPicker", false}
                };
        }

        private IDictionary<string, object> _defaultPreValues;

        protected override PropertyValueEditor CreateValueEditor()
        {
            //TODO: Need to add some validation to the ValueEditor to ensure that any media chosen actually exists!

            return base.CreateValueEditor();
            
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreValues; }
            set { _defaultPreValues = value; }
        }
    }
}
