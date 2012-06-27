using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.datatype
{
    public class DataEditorSettingValidationResult
    {
        public String ErrorMessage { get; set; }

        public DataEditorSettingValidationResult(string errorMessage)
        {
            ErrorMessage = errorMessage;

        }
    }
}
