using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MultiNodeTreePickerAlias, "Multinode Treepicker", "contentpicker")]
    public class MultiNodeTreePickerPropertyEditor : PropertyEditor
    {
        public MultiNodeTreePickerPropertyEditor()
        {
            _internalPreValues = new Dictionary<string, object>
                {
                    {"multiPicker", true}
                };
        }
  
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MultiNodePickerPreValueEditor();
        }

        private IDictionary<string, object> _internalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _internalPreValues; }
            set { _internalPreValues = value; }
        }

        internal class MultiNodePickerPreValueEditor : PreValueEditor
        {
            [PreValueField("startNode", "Node type", "treesource")]
            public string StartNode { get; set; }

            [PreValueField("multiPicker", "Pick multiple items", "boolean")]
            public bool MultiPicker { get; set; }
            
            [PreValueField("filter", "Filter out items with type", "textstring", Description = "Seperate with comma")]
            public string Filter { get; set; }

            [PreValueField("minNumber", "Minumum number of items", "number")]
            public string MinNumber { get; set; }

            [PreValueField("maxNumber", "Maximum number of items", "number")]
            public string MaxNumber { get; set; }

        }
    }
}
