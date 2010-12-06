#region namespace
using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.providers;
using System.Collections.Specialized; 
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
                user.ChangePassword(u.GetPassword(), tb_password.Text.Trim());

                u.LoginName = tb_login.Text;

                // Is it using the default membership provider
                if (Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] is UsersMembershipProvider)
                {
                    // Save user in membership provider
                    UsersMembershipUser umbracoUser = user as UsersMembershipUser;
                    umbracoUser.FullName = tb_name.Text.Trim();
                    Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].UpdateUser(umbracoUser);

                    // Save user details
                    u.Email = tb_email.Text.Trim();
                }
                else
                {
                    u.Name = tb_name.Text.Trim();
                    if (!(Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] is ActiveDirectoryMembershipProvider)) Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].UpdateUser(user);
                }

                u.Save();

                if (cb_newsletter.Checked)
                {
                    try
                    {
                        System.Net.WebClient client = new System.Net.WebClient();
                        NameValueCollection values = new NameValueCollection();
                        values.Add("name", tb_name.Text);
                        values.Add("email", tb_email.Text);

                        client.UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", values);

                    }
                    catch { /* fail in silence */ }
                }

                Helper.RedirectToNextStep(this.Page);                
            }
		}

        private void SubscribeToNewsLetter(string name, string email)
        {
            try
            {
                System.Net.WebClient client = new System.Net.WebClient();
                NameValueCollection values = new NameValueCollection();
                values.Add("name", name);
                values.Add("email", email);

                client.UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", values);

            }
            catch { /* fail in silence */ }
        }
	}
}
