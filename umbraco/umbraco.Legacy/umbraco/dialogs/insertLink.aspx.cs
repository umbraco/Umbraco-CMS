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
	/// Summary description for insertLink.
	/// </summary>
	public partial class insertLink : BasePages.UmbracoEnsuredPage
	{
		protected uicontrols.TabView tbv = new uicontrols.TabView();
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			if (Request.QueryString["linkName"] != null) 
			{
				string link = Request.QueryString["linkName"];
//				actualLink.Value = link;
//				if (link.IndexOf("{#") > -1)
//					linkText.Text = link.Substring(link.IndexOf(",")+1, link.Length-2-link.IndexOf(","));
//				else
					linkText.Text = link;
			}

			if (helper.Request("linkNewWindow") == "_blank")
				linkNewWindow.Checked = true;
				

			uicontrols.TabPage tp = tbv.NewTabPage(ui.Text("content"));
			tp.HasMenu = false;
			tp.Controls.Add(new LiteralControl("<iframe frameborder=\"0\" src=\"../TreeInit.aspx?app=content&amp;isDialog=true&amp;contextMenu=false\" style=\"LEFT: 0px; OVERFLOW: auto; WIDTH: 280px; POSITION: relative; TOP: 0px; HEIGHT: 220px; BACKGROUND-COLOR: white; border: none\"></iframe>"));
			uicontrols.TabPage tp2 = tbv.NewTabPage(ui.Text("media"));
			tp2.HasMenu = false;
			tp2.Controls.Add(new LiteralControl("<iframe frameborder=\"0\" src=\"../TreeInit.aspx?app=media&amp;isDialog=true&amp;contextMenu=false\" style=\"LEFT: 0px; OVERFLOW: auto; WIDTH: 280px; POSITION: relative; TOP: 0px; HEIGHT: 220px; BACKGROUND-COLOR: white; border: none\"></iframe>"));

			ph.Controls.Add(tbv);
		}

		public BusinessLogic.User GetUser() 
		{
			return base.getUser();
		}
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			tbv.ID="tabview1";
			tbv.Width = 300;
			tbv.Height = 250;

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
