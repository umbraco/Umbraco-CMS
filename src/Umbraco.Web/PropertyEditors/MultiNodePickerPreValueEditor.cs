using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class MultiNodePickerPreValueEditor : PreValueEditor
    {
        
        [PreValueField("type", "Type", "views/prevalueEditors/nodetype.html")]
        public string Type { get; set; }

        [PreValueField("startNode", "Start node", "views/prevalueEditors/treePicker.html")]
        public int StartNode { get; set; }

        [PreValueField("multiPicker", "Pick multiple items", "views/prevalueEditors/boolean.html")]
        public bool MultiPicker { get; set; }



        [PreValueField("filter", "Filter out items with type", "views/prevalueEditors/textstring.html", Description = "Seperate with comma")]
        public string Filter { get; set; }

        [PreValueField("minNumber", "Minumum number of items", "views/prevalueEditors/number.html")]
        public string MinNumber { get; set; }

        [PreValueField("maxNumber", "Maximum number of items", "views/prevalueEditors/number.html")]
        public string MaxNumber { get; set; }
       
    }
}
