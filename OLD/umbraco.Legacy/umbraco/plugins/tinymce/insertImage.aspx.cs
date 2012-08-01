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

namespace umbraco.presentation.tinymce
{
	public partial class insertImage : BasePages.UmbracoEnsuredPage
	{
		protected uicontrols.TabView tbv = new uicontrols.TabView();
		

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
			uicontrols.TabPage tp = tbv.NewTabPage(ui.Text("choose"));
			tp.HasMenu = false;
			tp.Controls.Add(new LiteralControl("<div style=\"padding: 5px;\"><iframe src=\"../../TreeInit.aspx?app=media&isDialog=true&dialogMode=id&contextMenu=false&functionToCall=parent.dialogHandler\" style=\"LEFT: 9px; OVERFLOW: auto; WIDTH: 200px; POSITION: relative; TOP: 0px; HEIGHT: 250px; BACKGROUND-COLOR: white\"></iframe>&nbsp;<iframe src=\"../../dialogs/imageViewer.aspx\" id=\"imageViewer\" style=\"LEFT: 9px; OVERFLOW: auto; WIDTH: 250px; POSITION: relative; TOP: 0px; HEIGHT: 250px; BACKGROUND-COLOR: white\"></iframe></div>"));
			uicontrols.TabPage tp2 = tbv.NewTabPage(ui.Text("create") + " " + ui.Text("new"));
			
			tp2.HasMenu = false;
			tp2.Controls.Add(new LiteralControl("<iframe frameborder=\"0\" src=\"../../dialogs/uploadImage.aspx\" style=\"LEFT: 0px; OVERFLOW: auto; WIDTH: 500px; POSITION: relative; TOP: 0px; HEIGHT: 220px; BACKGROUND-COLOR: white; border: none\"></iframe>"));
			PlaceHolder1.Controls.Add(tbv);
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			tbv.ID="tabview1";
			tbv.Width = 500;
			tbv.Height = 290;

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
