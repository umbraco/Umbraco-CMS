using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace umbraco.MacroEngines.RazorDataTypeModels
{
    public class HtmlStringDataTypeModel : IRazorDataTypeModel
    {
        public bool Init(int CurrentNodeId, string PropertyData, out object instance)
        {
            instance = new HtmlString(PropertyData);
            return true;
        }
    }
}
