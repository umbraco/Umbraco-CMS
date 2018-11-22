using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class ListBox : DataEditorSettingType
    {
        private System.Web.UI.WebControls.ListBox lb = new System.Web.UI.WebControls.ListBox();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
                return lb.SelectedValue;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _val = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {
            lb.ID = sender.GetName();

            lb.CssClass = "guiInputStandardSize";

           lb.Items.Clear();

            foreach (string s in Prevalues)
            {
                lb.Items.Add(s);
            }

            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue))
                lb.SelectedValue = DefaultValue;
            else
                lb.SelectedValue = _val;

            return lb;
        }
    }
}