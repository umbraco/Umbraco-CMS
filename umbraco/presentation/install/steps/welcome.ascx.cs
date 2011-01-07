using umbraco;

namespace umbraco.presentation.install
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for welcome.
	/// </summary>
	public partial class welcome : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{

            if (!String.IsNullOrEmpty(GlobalSettings.ConfigurationStatus.Trim()))
            {
                ph_install.Visible = false;
                ph_upgrade.Visible = true;
            }

			// Check for config!
              if (GlobalSettings.Configured)
              {
                    Application.Lock();
                    Application["umbracoNeedConfiguration"] = null;
                    Application.UnLock();
                    Response.Redirect(Request.QueryString["url"] ?? "/", true);
              }
              
              

		}

    protected void gotoNextStep(object sender, EventArgs e) {
      Helper.RedirectToNextStep(this.Page);
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
	}


}
