using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using umbraco.DataLayer;
using umbraco.interfaces;


namespace umbraco.macroRenderings
{
    
	/// <summary>
	/// Summary description for tabPicker.
	/// </summary>
	public class tabPicker : System.Web.UI.WebControls.ListBox, interfaces.IMacroGuiRendering
	{
		string _value = "";
		bool _multiple = false;
        private static ISqlHelper _sqlHelper;
        private static string _ConnString = GlobalSettings.DbDSN;

		public bool ShowCaption 
		{
			get {return true;}
		}

		public virtual bool Multiple 
		{
			set {_multiple = value;}
			get {return _multiple;}
		}

		public string Value 
		{
			get 
			{
				string retVal = "";
				foreach(System.Web.UI.WebControls.ListItem i in base.Items) 
					if (i.Selected)
						retVal += i.Value + ",";

				if (retVal != "")
					retVal = retVal.Substring(0, retVal.Length-1);

				return retVal;
			}
			set 
			{
				_value = value;
			}
		}

        public static ISqlHelper SqlHelper {
            get {
                if (_sqlHelper == null) {
                    try {
                        _sqlHelper = DataLayerHelper.CreateSqlHelper(_ConnString);
                    } catch { }
                }
                return _sqlHelper;
            }
        }

		public tabPicker()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
			
			this.CssClass = "guiInputTextStandard";

			// Check for multiple choises
			if (_multiple) 
			{
				this.SelectionMode = System.Web.UI.WebControls.ListSelectionMode.Multiple;
				this.Rows = 5;
				this.Multiple =true;
			} 
			else 
			{
				this.Rows = 1;
				this.Items.Add(new System.Web.UI.WebControls.ListItem("", ""));
				this.SelectionMode = System.Web.UI.WebControls.ListSelectionMode.Single;
			}

			//SqlDataReader dr = SqlHelper.ExecuteReader(GlobalSettings.DbDSN, CommandType.Text, "select distinct text from cmsTab order by text");



            using (IRecordsReader dr = SqlHelper.ExecuteReader("select distinct text from cmsTab order by text")) {
                while (dr.Read()) {

                    System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem(dr.GetString("text"), dr.GetString("text").ToLower());

                    if (((string)(", " + _value + ",")).IndexOf(", " + dr.GetString("text").ToLower() + ",") > -1)
                        li.Selected = true;

                    this.Items.Add(li);

                }
            }

/*
			while (dr.Read())
			{
				System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem(dr["text"].ToString(), dr["text"].ToString().ToLower());
				if (((string) (", "+_value+",")).IndexOf(", "+dr["text"].ToString().ToLower()+",") > -1)
					li.Selected = true;

				this.Items.Add(li);
			}
			dr.Close();*/

		}

	}

    
}
