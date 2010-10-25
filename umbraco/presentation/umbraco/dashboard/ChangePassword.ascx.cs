using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using umbraco.BusinessLogic;

namespace umbraco.presentation.umbraco.dashboard
{
    public partial class ChangePassword : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void changePassword_Click(object sender, EventArgs e)
        {
            User u = User.GetCurrent();
            MembershipUser user = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].GetUser(u.LoginName, true);


            string tempPassword = password.Text;
            if (!string.IsNullOrEmpty(tempPassword.Trim()))
            {
                if (tempPassword == confirmpassword.Text)
                {
                    // make sure password is not empty
                    user.ChangePassword(u.Password, tempPassword);
                    changeForm.Visible = false;
                    errorPane.Visible = false;
                    passwordChanged.Visible = true;
                }
                else
                {
                    errorPane.Visible = true;
                    errorMessage.Text = ui.Text("passwordIsDifferent");
                }
            }
            else
            {
                errorPane.Visible = true;
                errorMessage.Text = ui.Text("passwordIsBlank");
            }

        }
    }
}