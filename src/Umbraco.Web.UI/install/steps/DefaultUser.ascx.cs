using System;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;
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

        protected MembershipProvider CurrentProvider
        {
            get
            {
                var provider = Membership.Providers[UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider];
                if (provider == null)
                {
                    throw new InvalidOperationException("No MembershipProvider found with name " + UmbracoConfig.For.UmbracoSettings().Providers.DefaultBackOfficeUserProvider);
                }
                return provider;
            }
        }

        protected void ChangePasswordClick(object sender, System.EventArgs e)
        {
            Page.Validate();

            if (Page.IsValid)
            {
                var u = User.GetUser(0);
                var user = CurrentProvider.GetUser(0, true);
                if (user == null)
                {
                    throw new InvalidOperationException("No user found in membership provider with id of 0");
                }

                //NOTE: This will throw an exception if the membership provider 
                try
                {
                    var success = user.ChangePassword(u.GetPassword(), tb_password.Text.Trim());
                    if (!success)
                    {
                        PasswordValidator.IsValid = false;
                        PasswordValidator.ErrorMessage = "Password must be at least " + CurrentProvider.MinRequiredPasswordLength + " characters long and contain at least " + CurrentProvider.MinRequiredNonAlphanumericCharacters + " symbols";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    PasswordValidator.IsValid = false;
                    PasswordValidator.ErrorMessage = "Password must be at least " + CurrentProvider.MinRequiredPasswordLength + " characters long and contain at least " + CurrentProvider.MinRequiredNonAlphanumericCharacters + " symbols";
                    return;
                }

                // Is it using the default membership provider
                if (CurrentProvider.IsUmbracoUsersProvider())
                {
                    // Save user in membership provider
                    var umbracoUser = user as UsersMembershipUser;
                    umbracoUser.FullName = tb_name.Text.Trim();
                    CurrentProvider.UpdateUser(umbracoUser);

                    // Save user details
                    u.Email = tb_email.Text.Trim();
                }
                else
                {
                    u.Name = tb_name.Text.Trim();
                    if ((CurrentProvider is ActiveDirectoryMembershipProvider) == false)
                    {
                        CurrentProvider.UpdateUser(user);
                    }
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