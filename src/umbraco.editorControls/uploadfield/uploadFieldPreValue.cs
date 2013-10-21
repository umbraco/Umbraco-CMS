using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.editorControls.uploadfield
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    class uploadFieldPreValue : System.Web.UI.WebControls.PlaceHolder, interfaces.IDataPrevalue
	{
	
		// UI controls
        private TextBox _textboxThumbnails;
		private DropDownList _dropdownlist;
				
		// referenced datatype
		private cms.businesslogic.datatype.BaseDataType _datatype;

        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        public uploadFieldPreValue(cms.businesslogic.datatype.BaseDataType DataType) 
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


			_textboxThumbnails = new TextBox();
            _textboxThumbnails.ID = "thumbNailSizes";

			// put the childcontrols in context - ensuring that
			// the viewstate is persisted etc.
			this.Controls.Add(_dropdownlist);
            this.Controls.Add(_textboxThumbnails);

		}
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			if (!Page.IsPostBack)
			{
				string[] config = Configuration.Split("|".ToCharArray());
				if (config.Length > 0) 
				{
                    _textboxThumbnails.Text = config[0];
				}
                _dropdownlist.SelectedValue = _datatype.DBType.ToString();
			}
		}
		
		public Control Editor 
		{
			get
			{
				return this;
			}
		}

		public void Save() 
		{
            _datatype.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);

			// Generate data-string
            string data = _textboxThumbnails.Text;
			// If the add new prevalue textbox is filled out - add the value to the collection.
			IParameter[] SqlParams = new IParameter[] {
										SqlHelper.CreateParameter("@value",data),
										SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
			SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid",SqlParams);
            // need to unlock the parameters (for SQL CE compat)
            SqlParams = new IParameter[] {
										SqlHelper.CreateParameter("@value",data),
										SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
            SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')", SqlParams);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.WriteLine("<table>");
			writer.WriteLine("<tr><th>Database datatype</th><td>");
			_dropdownlist.RenderControl(writer);
			writer.Write("</td></tr>");
			writer.Write("<tr><th>Thumbnail sizes (max width/height, semicolon separated for multiples):</th><td>");
            _textboxThumbnails.RenderControl(writer);
			writer.Write("</td></tr>");
			writer.Write("</table>");
		}

		public string Configuration 
		{
			get 
			{
                object configVal = SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid", SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
                if (configVal != null)
                    return configVal.ToString();
                else
                    return "";
			}
		}

	}
}
