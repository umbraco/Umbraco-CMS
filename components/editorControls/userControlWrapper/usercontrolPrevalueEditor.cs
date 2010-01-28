using System;
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
using umbraco.IO;

namespace umbraco.editorControls.userControlGrapper
{
    public class usercontrolPrevalueEditor : System.Web.UI.WebControls.PlaceHolder, umbraco.interfaces.IDataPrevalue
    {
        public ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #region IDataPrevalue Members

        // referenced datatype
        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        private DropDownList _dropdownlist;
        private DropDownList _dropdownlistUserControl;


        public usercontrolPrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType DataType) 
		{
			// state it knows its datatypedefinitionid
			_datatype = DataType;
			setupChildControls();

		}
		
		private void setupChildControls() 
		{
			_dropdownlist = new DropDownList();
			_dropdownlist.ID = "dbtype";
			_dropdownlist.Items.Add(DBTypes.Date.ToString());
			_dropdownlist.Items.Add(DBTypes.Integer.ToString());
			_dropdownlist.Items.Add(DBTypes.Ntext.ToString());
			_dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());
			
			_dropdownlistUserControl = new DropDownList();
			_dropdownlistUserControl.ID = "usercontrol";

			// put the childcontrols in context - ensuring that
			// the viewstate is persisted etc.
			Controls.Add(_dropdownlist);
			Controls.Add(_dropdownlistUserControl);

            // populate the usercontrol dropdown
            _dropdownlistUserControl.Items.Add(new ListItem(ui.Text("choose"), ""));
            populateUserControls( IOHelper.MapPath( SystemDirectories.Usercontrols) );
			
		}

        private void populateUserControls(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (FileInfo uc in di.GetFiles("*.ascx"))
            {
                string root = IOHelper.MapPath(SystemDirectories.Root);

                _dropdownlistUserControl.Items.Add(
                    
                    new ListItem( 
                            uc.FullName.Substring(root.Length).Replace(IOHelper.DirSepChar, '/'))

                    /*
                    new ListItem( 
                        uc.FullName.Substring( uc.FullName.IndexOf(root), uc.FullName.Length - uc.FullName.IndexOf(root)).Replace(IOHelper.DirSepChar, '/'))
                      */  
                        );

            }
            foreach (DirectoryInfo dir in di.GetDirectories())
                populateUserControls(dir.FullName);
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
                string config = Configuration;
                if (config != "")
                {
                    _dropdownlistUserControl.SelectedValue = config;
                }
                _dropdownlist.SelectedValue = _datatype.DBType.ToString();

            }
        }

        public void Save()
        {
            _datatype.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);

            // Generate data-string
            string data = _dropdownlistUserControl.SelectedValue;

            // If the add new prevalue textbox is filled out - add the value to the collection.
            IParameter[] SqlParams = new IParameter[] {
			            SqlHelper.CreateParameter("@value",data),
						SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", SqlParams);
            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')", SqlParams);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>Database datatype</th><td>");
            _dropdownlist.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("<tr><th>Usercontrol:</th><td>");
            _dropdownlistUserControl.RenderControl(writer);
            writer.Write("</td></tr>");
            writer.Write("</table>");
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

        #endregion
    }
}
