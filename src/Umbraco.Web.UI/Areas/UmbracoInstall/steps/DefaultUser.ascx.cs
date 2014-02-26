using System;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
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

        protected void ChangePasswordClick(object sender, EventArgs e)
        {
            Page.Validate();

            if (Page.IsValid)
            {
                var user = User.GetUser(0);
                
                var membershipUser = CurrentProvider.GetUser(0, true);
                if (membershipUser == null)
                {
                    throw new InvalidOperationException("No user found in membership provider with id of 0");
                }
                
                try
                {
                    var success = membershipUser.ChangePassword(user.GetPassword(), tb_password.Text.Trim());
                    if (success == false)
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

                user.Email = tb_email.Text.Trim();
                user.Name = tb_name.Text.Trim();
                user.LoginName = tb_login.Text;
                
                user.Save();

                if (cb_newsletter.Checked)
                {
                    try
                    {
                        var client = new System.Net.WebClient();
                        var values = new NameValueCollection {{"name", tb_name.Text}, {"email", tb_email.Text}};

                        client.UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", values);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<DefaultUser>("An error occurred subscribing user to newsletter", ex);
                    }
                }


                if (String.IsNullOrWhiteSpace(GlobalSettings.ConfigurationStatus))
                    UmbracoContext.Current.Security.PerformLogin(user.Id);

                //InstallHelper.RedirectToNextStep(Page, GetCurrentStep());
            }
        }

    }
}