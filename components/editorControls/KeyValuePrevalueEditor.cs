using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for KeyValuePrevalueEditor.
	/// </summary>
	public class KeyValuePrevalueEditor : System.Web.UI.WebControls.PlaceHolder, interfaces.IDataPrevalue
	{
	
		// UI controls
		public System.Web.UI.WebControls.DropDownList _dropdownlist;
		public TextBox _textbox;
        public umbraco.uicontrols.PropertyPanel pp1 = new umbraco.uicontrols.PropertyPanel();
        public umbraco.uicontrols.PropertyPanel pp2 = new umbraco.uicontrols.PropertyPanel();
				
		private Hashtable DeleteButtons = new Hashtable();

		// referenced datatype
		private cms.businesslogic.datatype.BaseDataType _datatype;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		public KeyValuePrevalueEditor(cms.businesslogic.datatype.BaseDataType DataType) 
		{
			// state it knows its datatypedefinitionid
			_datatype = DataType;

            setupChildControls();
            
            // Bootstrap delete.
			if (System.Web.HttpContext.Current.Request["delete"] != null) {
				DeletePrevalue(int.Parse(System.Web.HttpContext.Current.Request["delete"]));
			}
			
		}
		
		private void DeletePrevalue(int id) {
			SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where id = " + id);
		}

		private void setupChildControls() 
		{
			_dropdownlist = new DropDownList();
			_dropdownlist.ID = "dbtype";
			
			_textbox = new TextBox();
			_textbox.ID = "AddValue";

			// put the childcontrols in context - ensuring that
			// the viewstate is persisted etc.
			this.Controls.Add(_dropdownlist);
			this.Controls.Add(_textbox);

			_dropdownlist.Items.Add(DBTypes.Date.ToString());
			_dropdownlist.Items.Add(DBTypes.Integer.ToString());
			_dropdownlist.Items.Add(DBTypes.Ntext.ToString());
			_dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
        
            if (!Page.IsPostBack)
			{
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
            _datatype.DBType = (cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(cms.businesslogic.datatype.DBTypes), _dropdownlist.SelectedValue, true);
			// If the add new prevalue textbox is filled out - add the value to the collection.
			if (_textbox.Text != "") 
			{
				IParameter[] SqlParams = new IParameter[] {
								SqlHelper.CreateParameter("@value",_textbox.Text),
								SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId)};
				SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')",SqlParams);
				_textbox.Text = "";

                ScriptManager.GetCurrent(Page).SetFocus(_textbox);
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("dataBaseDatatype") + "</div>");
            _dropdownlist.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");

            SortedList _prevalues = Prevalues;
            if (_prevalues.Count > 0) {
                writer.Write("<div class='propertyItem'><table style='width: 100%'>");
                writer.Write("<tr><th style='width: 15%'>Text</th><td colspan='2'>Value</td></tr>");

                foreach (object key in _prevalues.Keys) {
                    writer.Write("<tr><td>" + _prevalues[key].ToString() + "</td><td> " + key + "</td><td><a href='?id=" + _datatype.DataTypeDefinitionId + "&delete=" + key.ToString() + "'>" + ui.Text("delete") + "</a></td></tr>");
                }
                writer.Write("</table><br style='clear: both'/></div>");
            }

            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("addPrevalue") + "</div>");
            _textbox.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");
            
            
        
        }

		public SortedList Prevalues {
			get
            {
                SortedList retval = new SortedList();
				IRecordsReader dr = SqlHelper.ExecuteReader(
					"Select id, [value] from cmsDataTypePreValues where DataTypeNodeId = "
					+ _datatype.DataTypeDefinitionId + " order by sortorder");
				
				while (dr.Read())
					retval.Add(dr.GetInt("id"), dr.GetString("value"));
				dr.Close();
				return retval;
			}
		}
	}
}
