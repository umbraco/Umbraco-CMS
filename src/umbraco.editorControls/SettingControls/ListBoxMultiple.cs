using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class ListBoxMultiple : DataEditorSettingType
    {
        private System.Web.UI.WebControls.ListBox lb = new System.Web.UI.WebControls.ListBox();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
                string retVal = string.Empty;

                foreach (System.Web.UI.WebControls.ListItem item in lb.Items)
                {
                    if (item.Selected)
                        retVal += item.Value + ";";
                }
                return retVal;

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
            lb.SelectionMode = System.Web.UI.WebControls.ListSelectionMode.Multiple;
            lb.CssClass = "guiInputStandardSize";
            lb.Items.Clear();

            foreach (string s in Prevalues)
            {
                System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(s);

                if (_val.Contains(s + ";"))
                    item.Selected = true;
                
                lb.Items.Add(item);

            }

            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue))
                lb.SelectedValue = DefaultValue;

            return lb;
        }
    }
}