using System;
using System.Collections;
using System.Linq;

namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for propertyTypePicker.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class PropertyTypePicker : System.Web.UI.WebControls.ListBox, interfaces.IMacroGuiRendering
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

			Hashtable ht = new Hashtable();
			foreach(cms.businesslogic.propertytype.PropertyType pt in cms.businesslogic.propertytype.PropertyType.GetAll().OrderBy(x => x.Name))
			{
				if (!ht.ContainsKey(pt.Alias)) 
				{
					System.Web.UI.WebControls.ListItem li = new System.Web.UI.WebControls.ListItem(pt.Alias);
					if (((string) (", "+_value+",")).IndexOf(", "+pt.Alias+",") > -1)
						li.Selected = true;
					ht.Add(pt.Alias, "");

					this.Items.Add(li);
				}
			}

		}

	}
}
