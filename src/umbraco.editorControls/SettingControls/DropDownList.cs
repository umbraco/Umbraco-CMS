using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
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

            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue))
                ddl.SelectedValue = DefaultValue;
            else
                ddl.SelectedValue = _val;

            return ddl;
        }
    }
}