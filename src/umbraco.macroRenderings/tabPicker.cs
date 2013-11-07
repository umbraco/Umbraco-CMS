using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.BusinessLogic;
using Umbraco.Core;


namespace umbraco.macroRenderings
{
    
	/// <summary>
	/// Summary description for tabPicker.
	/// </summary>
	public class tabPicker : System.Web.UI.WebControls.ListBox, interfaces.IMacroGuiRendering
	{
		string _value = "";
		bool _multiple = false;

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

        public static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
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

            var propertyTypeGroups = ApplicationContext.Current.DatabaseContext.Database.Fetch<string>(
                "select distinct text from cmsPropertyTypeGroup order by text");
            foreach (var propertyTypeGroup in propertyTypeGroups)
            {
                System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem(propertyTypeGroup, propertyTypeGroup.ToLower());

                if (((string)(", " + _value + ",")).IndexOf(", " + propertyTypeGroup.ToLower() + ",") > -1)
                {
                    li.Selected = true;
                }

                this.Items.Add(li);                
            }
		}
	}    
}
