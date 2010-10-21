using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.datatype
{
    public interface IDataEditorSettingType
    {
        string Value { get; set; }
        string DefaultValue { get; set; }
        System.Web.UI.Control RenderControl(DataEditorSetting setting);
    }
}
