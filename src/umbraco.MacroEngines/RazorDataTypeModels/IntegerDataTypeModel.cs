using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;

namespace umbraco.MacroEngines.RazorDataTypeModels
{
    [RazorDataTypeModel(Constants.PropertyEditors.Integer, 90)]
    [RazorDataTypeModel("f231cd8a-e447-424a-94a4-bc73b11736bb", 90)]
    [RazorDataTypeModel(Constants.PropertyEditors.RadioButtonList, 90)]
    [RazorDataTypeModel(Constants.PropertyEditors.MemberPicker, 90)]
    [RazorDataTypeModel("f6524852-2fb0-11e0-a9fa-6f1cdfd72085", 90)] //Extended Content Picker https://our.umbraco.com/projects/backoffice-extensions/extended-content-picker
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
