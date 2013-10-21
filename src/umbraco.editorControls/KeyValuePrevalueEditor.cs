using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;



[assembly: System.Web.UI.WebResource("umbraco.editorControls.KeyValuePrevalueEditor.js", "text/js")]
[assembly: System.Web.UI.WebResource("umbraco.editorControls.KeyValuePrevalueEditor.css", "text/css")]
namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for KeyValuePrevalueEditor.
	/// </summary>

    [ClientDependency(ClientDependencyType.Javascript, "Jeditable/jquery.jeditable.js", "UmbracoClient")]
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class KeyValuePrevalueEditor : System.Web.UI.WebControls.PlaceHolder, interfaces.IDataPrevalue
	{
	
		// UI controls
		public System.Web.UI.WebControls.DropDownList _dropdownlist;
		public TextBox _textbox;
        private TextBox _tbhidden;
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

            _tbhidden = new TextBox();
            _tbhidden.Attributes.Add("style", "display:none;");
            _tbhidden.CssClass = "valuesHiddenInput";

			// put the childcontrols in context - ensuring that
			// the viewstate is persisted etc.
			this.Controls.Add(_dropdownlist);
			this.Controls.Add(_textbox);
            this.Controls.Add(_tbhidden);

			_dropdownlist.Items.Add(DBTypes.Date.ToString());
			_dropdownlist.Items.Add(DBTypes.Integer.ToString());
			_dropdownlist.Items.Add(DBTypes.Ntext.ToString());
			_dropdownlist.Items.Add(DBTypes.Nvarchar.ToString());
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);


            System.Web.UI.Page page = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;


            page.ClientScript.RegisterClientScriptInclude(
                "umbraco.editorControls.KeyValuePrevalueEditor.js",
                page.ClientScript.GetWebResourceUrl(typeof(KeyValuePrevalueEditor), "umbraco.editorControls.KeyValuePrevalueEditor.js"));


            HtmlHead head = (HtmlHead)page.Header;
            HtmlLink link = new HtmlLink();
            link.Attributes.Add("href", page.ClientScript.GetWebResourceUrl(typeof(KeyValuePrevalueEditor), "umbraco.editorControls.KeyValuePrevalueEditor.css"));
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("rel", "stylesheet");
            head.Controls.Add(link);

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
			
            //changes in name and sortorder
            if (!string.IsNullOrEmpty(_tbhidden.Text))
            {
                int so = 0;
                foreach (string row in _tbhidden.Text.Split('¶'))
                {
                    if (!string.IsNullOrEmpty(row))
                    {
                     
                        int id = 0;

                        if (row.Split('|').Length == 2 &&  int.TryParse(row.Split('|')[0], out id) && row.Split('|')[1].Length > 0)
                        {

                            IParameter[] SqlParams = new IParameter[] {
								SqlHelper.CreateParameter("@value",row.Split('|')[1]),
								SqlHelper.CreateParameter("@sortorder",so),
                                SqlHelper.CreateParameter("@id",id)};
                            SqlHelper.ExecuteNonQuery("update cmsDataTypePreValues set [value] = @value, sortorder = @sortorder where id = @id", SqlParams);

                        }

                        so++;
                    }
                }

                _tbhidden.Text = "";
            }

            // If the add new prevalue textbox is filled out - add the value to the collection.
			if (_textbox.Text != "") 
			{

                int so = -1;

                try
                {
                    so = SqlHelper.ExecuteScalar<int>("select max(sortorder) from cmsDataTypePreValues where datatypenodeid = @dtdefid",
                        SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));
                    so++;
                }
                catch { }

				IParameter[] SqlParams = new IParameter[] {
								SqlHelper.CreateParameter("@value",_textbox.Text),
								SqlHelper.CreateParameter("@dtdefid",_datatype.DataTypeDefinitionId),
                                SqlHelper.CreateParameter("@so",so)};
				SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,@so,'')",SqlParams);
				_textbox.Text = "";

                ScriptManager.GetCurrent(Page).SetFocus(_textbox);
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("dataBaseDatatype") + "</div>");
            _dropdownlist.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");

            List<KeyValuePair<int, String>> _prevalues = PrevaluesAsKeyValuePairList;
            if (_prevalues.Count > 0) {
                writer.Write("<div class='propertyItem'><table style='width: 100%' id=\"prevalues\">");
                writer.Write("<tr class='header'><th style='width: 15%'>Text</th><td colspan='2'>Value</td></tr>");

                foreach (KeyValuePair<int, String> item in _prevalues)
                {
                    writer.Write("<tr class=\"row\"><td class=\"text\">" + item.Value + "</td><td class=\"value\"> " + item.Key.ToString() + "</td><td><a onclick='javascript:return ConfirmPrevalueDelete();' href='?id=" + _datatype.DataTypeDefinitionId + "&delete=" + item.Key.ToString() + "'>" + ui.Text("delete") + "</a> <span class=\"handle\" style=\"cursor:move\">sort<span></td></tr>");
                }
                writer.Write("</table><br style='clear: both'/></div>");
            }

            writer.Write("<div class='propertyItem'><div class='propertyItemheader'>" + ui.Text("addPrevalue") + "</div>");
            _textbox.RenderControl(writer);
            writer.Write("<br style='clear: both'/></div>");

            _tbhidden.RenderControl(writer);

        
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

        public List<KeyValuePair<int, String>> PrevaluesAsKeyValuePairList
        {
            get
            {

                List<KeyValuePair<int, String>> items = new List<KeyValuePair<int, String>>();

                IRecordsReader dr = SqlHelper.ExecuteReader(
                    "Select id, [value] from cmsDataTypePreValues where DataTypeNodeId = "
                    + _datatype.DataTypeDefinitionId + " order by sortorder");

                while (dr.Read())
                    items.Add(new KeyValuePair<int, string>(dr.GetInt("id"), dr.GetString("value")));
                dr.Close();
                return items;
            }
        }
	}
}
