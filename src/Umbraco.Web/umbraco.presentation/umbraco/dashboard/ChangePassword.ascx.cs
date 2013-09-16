using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Umbraco.Core.Configuration;
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
            MembershipProvider provider = Membership.Providers[UmbracoConfiguration.Current.UmbracoSettings.Providers.Users.DefaultBackOfficeProvider];
            MembershipUser user = provider.GetUser(u.LoginName, true);


            string newPass = password.Text;
            string oldPass = currentpassword.Text;
            if (!string.IsNullOrEmpty(oldPass) && provider.ValidateUser(u.LoginName, oldPass))
            {
                if (!string.IsNullOrEmpty(newPass.Trim()))
                {
                    if (newPass == confirmpassword.Text)
                    {
                        // make sure password is not empty
                        user.ChangePassword(u.Password, newPass);
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
            else
            {
                errorPane.Visible = true;
                errorMessage.Text = ui.Text("passwordInvalid");
            }

        }
    }
}