using System;
using System.Data;
using System.Data.SqlClient;


namespace umbraco.macroRenderings
{
	/// <summary>
	/// Summary description for contentType.
	/// </summary>
	public class contentTypeSingle : System.Web.UI.WebControls.ListBox, interfaces.IMacroGuiRendering
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
			get {
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

		public contentTypeSingle()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
			
			int count = 0;
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
				count = 1;
			}

			// This should be replaced by business logic, but right now it would cause a circular reference if
			// umbRuntime was added as a reference to this project
			foreach(cms.businesslogic.web.DocumentType dt in cms.businesslogic.web.DocumentType.GetAllAsList())
			{
				System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem(dt.Text, dt.Id.ToString());
				if (((string) (", "+_value+",")).IndexOf(", "+dt.Id.ToString()+",") > -1)
					li.Selected = true;

				this.Items.Add(li);
				count++;
			}
		}

	}
}
