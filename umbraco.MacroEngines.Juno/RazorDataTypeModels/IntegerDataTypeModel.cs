using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines.RazorDataTypeModels
{
    [RazorDataTypeModel("1413afcb-d19a-4173-8e9a-68288d2a73b8", 90)]
    [RazorDataTypeModel("f231cd8a-e447-424a-94a4-bc73b11736bb", 90)]
    [RazorDataTypeModel("a52c7c1c-c330-476e-8605-d63d3b84b6a6", 90)]
    //[RazorDataTypeModel("22c6f52c-5d41-45b8-845e-c16354cfdd7b", 90)]
    [RazorDataTypeModel("39f533e4-0551-4505-a64b-e0425c5ce775", 90)]
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
