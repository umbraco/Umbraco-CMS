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
        [PreValueField("multiPicker", "Pick multiple items", "views/prevalueEditors/boolean.html")]
        public bool MultiPicker { get; set; }

        [PreValueField("type", "Type", "views/prevalueEditors/nodetype.html")]
        public string Type { get; set; }

        [PreValueField("filter", "Filter by content type", "views/prevalueEditors/textstring.html", Description = "Seperate with comma")]
        public string Filter { get; set; }
       
    }
}
