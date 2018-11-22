using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls.Pickers
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class BasePicker: DataEditorSettingType
    {
        public Guid ObjectGuid { get; set; }

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
            List<KeyValuePair<String, String>> items = new List<KeyValuePair<String, String>>();

            if (ObjectGuid != Guid.Empty)
            {
                Guid[] guids = umbraco.cms.businesslogic.CMSNode.getAllUniquesFromObjectType(ObjectGuid);
                foreach (Guid g in guids)
                {
                    umbraco.cms.businesslogic.CMSNode node = new umbraco.cms.businesslogic.CMSNode(g);
                    items.Add(new KeyValuePair<string, string>(node.Id.ToString(), node.Text));
                }
            }

            items.Sort(delegate(KeyValuePair<String, String> x, KeyValuePair<String, String> y) { return x.Value.CompareTo(y.Value); });

            foreach (KeyValuePair<String, String> kv in items)
            {
                ddl.Items.Add(new System.Web.UI.WebControls.ListItem(kv.Value, kv.Key));
            }

            System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem("Choose...", "");
            li.Selected = true;
            ddl.Items.Insert(0, li);


            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue))
                ddl.SelectedValue = DefaultValue;
            else
                ddl.SelectedValue = _val;

            return ddl;
        }
    }
}