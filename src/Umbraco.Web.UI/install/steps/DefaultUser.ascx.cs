using System;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install;
using Umbraco.Web.Security;
using umbraco.BusinessLogic;
using umbraco.providers;
using System.Collections.Specialized;

namespace Umbraco.Web.UI.Install.Steps
{
    /// <summary>
    ///		Summary description for defaultUser.
    /// </summary>
    public partial class DefaultUser : StepUserControl
    {

        protected void ChangePasswordClick(object sender, System.EventArgs e)
        {
            Page.Validate();

            if (Page.IsValid)
            {
                var u = User.GetUser(0);
                var user = Membership.Providers[LegacyUmbracoSettings.DefaultBackofficeProvider].GetUser(0, true);
                user.ChangePassword(u.GetPassword(), tb_password.Text.Trim());

                // Is it using the default membership provider
                if (Membership.Providers[LegacyUmbracoSettings.DefaultBackofficeProvider] is UsersMembershipProvider)
                {
                    // Save user in membership provider
                    var umbracoUser = user as UsersMembershipUser;
                    umbracoUser.FullName = tb_name.Text.Trim();
                    Membership.Providers[LegacyUmbracoSettings.DefaultBackofficeProvider].UpdateUser(umbracoUser);

                    // Save user details
                    u.Email = tb_email.Text.Trim();
                }
                else
                {
                    u.Name = tb_name.Text.Trim();
                    if (!(Membership.Providers[LegacyUmbracoSettings.DefaultBackofficeProvider] is ActiveDirectoryMembershipProvider)) Membership.Providers[LegacyUmbracoSettings.DefaultBackofficeProvider].UpdateUser(user);
                }

                // we need to update the login name here as it's set to the old name when saving the user via the membership provider!
                u.LoginName = tb_login.Text;

                u.Save();

                if (cb_newsletter.Checked)
                {
                    try
                    {
                        var client = new System.Net.WebClient();
                        var values = new NameValueCollection {{"name", tb_name.Text}, {"email", tb_email.Text}};

                        client.UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", values);

                    }
                    catch { /* fail in silence */ }
                }


                if (String.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus))
                    UmbracoContext.Current.Security.PerformLogin(u.Id);

                InstallHelper.RedirectToNextStep(Page, GetCurrentStep());
            }
        }

    }
}