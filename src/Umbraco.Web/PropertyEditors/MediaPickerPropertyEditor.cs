using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;


namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MediaPickerAlias, "Legacy Media Picker", "INT", "mediapicker", Group="media", Icon="icon-picture")]
    public class MediaPickerPropertyEditor : PropertyEditor
    {
        public MediaPickerPropertyEditor()
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
                //TODO: Need to add some validation to the ValueEditor to ensure that any media chosen actually exists!
                return base.CreateValueEditor();    
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

        internal class SingleMediaPickerPreValueEditor : PreValueEditor
        {
            [PreValueField("startNodeId", "Start node", "mediapicker")]
            public int StartNodeId { get; set; }
        }
    }
}
