using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines.RazorDataTypeModels
{
    [RazorDataTypeModel("1413afcb-d19a-4173-8e9a-68288d2a73b8", 90)]
    public class IntegerDataTypeModel : IRazorDataTypeModel
    {
        public bool Init(int CurrentNodeId, string PropertyData, out object instance)
        {
            int integer = 0;
            if (int.TryParse(PropertyData, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out integer))
            {
                instance = integer;
                return true;
            }
            instance = null;
            return false;
        }
    }
}
