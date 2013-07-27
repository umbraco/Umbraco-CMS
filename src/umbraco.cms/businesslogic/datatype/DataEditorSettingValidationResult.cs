using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.datatype
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public class DataEditorSettingValidationResult
    {
        public String ErrorMessage { get; set; }

        public DataEditorSettingValidationResult(string errorMessage)
        {
            ErrorMessage = errorMessage;

        }
    }
}
