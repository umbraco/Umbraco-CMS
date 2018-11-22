using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.datatype
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    public interface IDataEditorSettingType
    {
        string Value { get; set; }
        string DefaultValue { get; set; }
        System.Web.UI.Control RenderControl(DataEditorSetting setting);

        bool IsRequired { get; set; }
        string RegexValidationStatement { get; set; }

        DataEditorSettingValidationResult Validate();
    }
}
