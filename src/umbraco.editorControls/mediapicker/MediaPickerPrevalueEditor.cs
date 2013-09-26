using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using umbraco.DataLayer;
using umbraco.BusinessLogic;
using umbraco.editorControls;

namespace umbraco.editorControls.mediapicker
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MediaPickerPrevalueEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
    {

        public ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        //private DropDownList _dropdownlist;
        private CheckBox _checkboxShowPreview;
        private CheckBox _checkboxShowAdvancedDialog;

        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        public MediaPickerPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType DataType)
        {
            _datatype = DataType;
            setupChildControls();
        }

        private void setupChildControls()
        {
            //_dropdownlist = new DropDownList();
            //_dropdownlist.ID = "dbtype";
            //_dropdownlist.Items.Add(DBTypes.Date.ToString());
            //_dropdownlist.Items.Add(DBTypes.Integer.ToString());
            //_dropdownlist.Items.Add(DBTypes.Ntext.ToString());
            //_dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());

            _checkboxShowPreview = new CheckBox();
            _checkboxShowPreview.ID = "showpreview";

            _checkboxShowAdvancedDialog = new CheckBox();
            _checkboxShowAdvancedDialog.ID = "showadvanceddialog";

            //Controls.Add(_dropdownlist);

            Controls.Add(_checkboxShowPreview);
            Controls.Add(_checkboxShowAdvancedDialog);

        }

        public Control Editor
        {
            get
            {
                return this;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Page.IsPostBack)
            {

                _checkboxShowPreview.Checked = ShowPreview;
                _checkboxShowAdvancedDialog.Checked = ShowAdvancedDialog;

            }


        }

        public void Save()
        {
            _datatype.DBType = umbraco.cms.businesslogic.datatype.DBTypes.Integer;

            // Generate data-string
            string data = _checkboxShowPreview.Checked.ToString() + "|" + _checkboxShowAdvancedDialog.Checked.ToString();

            // If the add new prevalue textbox is filled out - add the value to the collection.
            IParameter[] SqlParams = new IParameter[] {
			            SqlHelper.CreateParameter("@value",data),
						SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", SqlParams);
            
            // For SQL CE 4 support we need this again!
            SqlParams = new IParameter[] {
			            SqlHelper.CreateParameter("@value",data),
						SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')", SqlParams);
        }

        public string Configuration
        {
            get
            {
                object conf =
                    SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid",
                                            SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
                if (conf != null)
                    return conf.ToString();
                else
                    return "";

            }
        }

        public bool ShowPreview
        {
            get
            {
                string[] config = Configuration.Split("|".ToCharArray());
                if (config.Length > 1)
                {
                    return Convert.ToBoolean(config[0]);
                }
                else
                {
                    return false;
                }

            }
        }

        public bool ShowAdvancedDialog
        {
            get
            {
                string[] config = Configuration.Split("|".ToCharArray());
                if (config.Length > 1)
                {
                    return Convert.ToBoolean(config[1]);
                }
                else
                {
                    return false;
                }

            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteLine("<table>");
            //writer.WriteLine("<tr><th>Database datatype:</th><td>");
            //_dropdownlist.RenderControl(writer);
            //writer.Write("</td></tr>");
            writer.Write("<tr><th>Show preview:</th><td>");
            _checkboxShowPreview.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>Show advanced dialog:</th><td>");
            _checkboxShowAdvancedDialog.RenderControl(writer);
            writer.Write("</td></tr>");           
            writer.Write("</table>");
        }
    }
}
