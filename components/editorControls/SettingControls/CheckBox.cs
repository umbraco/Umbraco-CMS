using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    public class CheckBox: DataEditorSettingType
    {
        private System.Web.UI.WebControls.CheckBox cb = new System.Web.UI.WebControls.CheckBox();

        public override string Value
        {
            get
            {
                return cb.Checked.ToString();
            }
            set
            {
                if (value == true.ToString())
                    cb.Checked = true;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {
            cb.ID = sender.GetName();
            return cb;
        }
    }
}