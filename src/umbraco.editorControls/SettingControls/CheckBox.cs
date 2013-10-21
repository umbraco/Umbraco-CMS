using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class CheckBox: DataEditorSettingType
    {
        private System.Web.UI.WebControls.CheckBox cb = new System.Web.UI.WebControls.CheckBox();

        private string _val = string.Empty;

        public override string Value
        {
            get
            {
                return cb.Checked.ToString();
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _val = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {
            cb.ID = sender.GetName();

            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue) && DefaultValue == true.ToString())
                cb.Checked = true;
            else if(_val == true.ToString())
                cb.Checked = true;

            return cb;
        }
    }
}