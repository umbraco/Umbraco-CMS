using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class CheckBoxList : DataEditorSettingType
    {
        private System.Web.UI.WebControls.CheckBoxList cbl = new System.Web.UI.WebControls.CheckBoxList();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
                string retVal = string.Empty;

                foreach (System.Web.UI.WebControls.ListItem item in cbl.Items)
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
            cbl.ID = sender.GetName();

            cbl.Items.Clear();

            foreach (string s in Prevalues)
            {
                System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(s);

                if(_val.Contains(s + ";"))
                    item.Selected = true;

                cbl.Items.Add(item);
                


            }

            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue))
                cbl.SelectedValue = DefaultValue;


            return cbl;
        }
    }
}