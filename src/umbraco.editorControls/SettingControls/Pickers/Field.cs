using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.DataLayer;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls.Pickers
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class Field : DataEditorSettingType
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
            List<KeyValuePair<String, String>> items = new List<KeyValuePair<String, String>>();


            string[] preValuesSource = { "@createDate", "@creatorName", "@level", "@nodeType", "@nodeTypeAlias", "@pageID", "@pageName", "@parentID", "@path", "@template", "@updateDate", "@writerID", "@writerName" };

            string fieldSql = "select distinct alias from cmsPropertyType order by alias";

            IRecordsReader dataTypes = umbraco.BusinessLogic.Application.SqlHelper.ExecuteReader(fieldSql);
            ddl.DataTextField = "alias";
            ddl.DataValueField = "alias";
            ddl.DataSource = dataTypes;
            ddl.DataBind();

            foreach (string s in preValuesSource)
            {
                ddl.Items.Add(new System.Web.UI.WebControls.ListItem(s, s.Replace("@", "")));
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