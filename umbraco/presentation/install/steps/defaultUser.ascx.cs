#region namespace
using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using umbraco.BusinessLogic; 
#endregion

namespace umbraco.presentation.install.steps
{
	/// <summary>
	///		Summary description for defaultUser.
	/// </summary>
	public partial class defaultUser : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Disable back/forward buttons
			Page.FindControl("next").Visible = false;
		
			BusinessLogic.User u = BusinessLogic.User.GetUser(0);

			if (u.NoConsole || u.Disabled) 
			{
				identifyResult.Text = "<div class='success'><p><strong>The Default user has been disabled or has no access to umbraco!</strong></p><p>No further actions needs to be taken. Click <b>Next</b> to proceed.</p></div>";
				Page.FindControl("next").Visible = true;
			}
			else if (u.GetPassword() != "default") 
			{
                identifyResult.Text = "<div class='success'><p><strong>The Default user's password has been successfully changed since the installation!</strong></p><p>No further actions needs to be taken. Click <strong>Next</strong> to proceed.</p></div>";
				Page.FindControl("next").Visible = true;
			}
			else 
			{
                identifyResult.Text = "<div class='notice'><p><strong>The Default users’ password needs to be changed!</strong></p></div>";
				changeForm.Visible = true;
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

		}
		#endregion

		protected void changePassword_Click(object sender, System.EventArgs e)
		{
            if (Page.IsValid)
            {
                User u = User.GetUser(0);
                MembershipUser user = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].GetUser(0, true);
                user.ChangePassword(u.GetPassword(), password.Text.Trim());

                passwordChanged.Visible = true;
                identify.Visible = false;
                changeForm.Visible = false;
                Page.FindControl("next").Visible = true;
            }
		}
	}
}
