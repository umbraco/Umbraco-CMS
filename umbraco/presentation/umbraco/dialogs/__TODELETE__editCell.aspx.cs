using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for editCell.
	/// </summary>
	public partial class editCell : BasePages.UmbracoEnsuredPage
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			string[] aligns = {"", "left", "center", "right"};
			string[] valigns = {"", "top", "middle", "bottom"};

			foreach(string s in aligns) 
			{
				ListItem li = new ListItem(s, s);
				if (s == helper.Request("align").ToLower())
					li.Selected = true;
				tableAlign.Items.Add(li);
			}
			foreach(string s in valigns) 
			{
				ListItem li = new ListItem(s, s);
				if (s == helper.Request("valign").ToLower())
					li.Selected = true;
				tableVAlign.Items.Add(li);
			}

		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}
}
