using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class RadioButtonList: DataEditorSettingType
    {
        private System.Web.UI.WebControls.RadioButtonList rbl = new System.Web.UI.WebControls.RadioButtonList();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
                return rbl.SelectedValue;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _val = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {
            rbl.ID = sender.GetName();

            rbl.Items.Clear();

            foreach (string s in Prevalues)
            {
                rbl.Items.Add(s);
            }

            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue))
                rbl.SelectedValue = DefaultValue;
            else
                rbl.SelectedValue = _val;

            return rbl;
        }
    }
}