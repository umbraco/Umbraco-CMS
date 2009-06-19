using System;

namespace umbraco.controls
{
	/// <summary>
	/// Summary description for progressBar.
	/// </summary>
	public class progressBar : System.Web.UI.WebControls.WebControl
	{
		public progressBar()
		{
		}

		protected override void OnInit(EventArgs e)
		{
			Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "progressBar", "<script language='javascript' src='/umbraco_client/progressBar/javascript.js'></script>");
			Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "progressBarCss", "<LINK href=\"/umbraco_client/progressBar/style.css\" type=\"text/css\" rel=\"stylesheet\">");
			base.OnInit (e);
		}


		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			writer.WriteLine("<div class=\"progressBar\"><table border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tr><td><div class=\"progressContainerLeft\" id=\"progressBar" + this.ClientID + "\" style=\"width: " + ((int) this.Width.Value+10).ToString() + "px\"><div class=\"progressIndicator\" style=\"width: 0px\"><img src=\"/umbraco/images/nada.gif\" id=\"progressBar" + this.ClientID + "_indicator\" width=\"0\" height=\"0\"/></div></div><div class=\"progressContainerRight\"><img src=\"/umbraco/images/nada.gif\" width=\"1\" height=\"1\"/></div><br style=\"clear: both\"/><span id=\"progressBar" + this.ClientID + "_text\" class=\"guiDialogTiny\" style=\"margin-top: 2px; width: 100%; text-align: center\"></span></td></tr></table><br class=\"guiDialogTiny\" style=\"clear: both\"/></div>");
		}


	}
}
