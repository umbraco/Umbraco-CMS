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
       
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MultiNodePickerPreValueEditor();
        }

        internal class MultiNodePickerPreValueEditor : PreValueEditor
        {

            [PreValueField("type", "Type", "nodetype")]
            public string Type { get; set; }

            [PreValueField("startNode", "Start node", "treepicker")]
            public int StartNode { get; set; }

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
