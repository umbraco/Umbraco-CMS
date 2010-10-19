using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    public class DropDownList: DataEditorSettingType
    {
        private System.Web.UI.WebControls.DropDownList ddl = new System.Web.UI.WebControls.DropDownList();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
                return ddl.SelectedValue;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _val = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {
            ddl.ID = sender.GetName();

            ddl.Items.Clear();

            foreach (string s in Prevalues)
            {
                ddl.Items.Add(s);
            }

            ddl.SelectedValue = _val;

            return ddl;
        }
    }
}