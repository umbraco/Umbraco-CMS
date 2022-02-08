using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Cms.Web.UI.Security
{
    public class GoogleBackOfficeExternalLoginProviderOptions : IConfigureNamedOptions<BackOfficeExternalLoginProviderOptions>
    {
        public const string SchemeName = "Google";

        public void Configure(string name, BackOfficeExternalLoginProviderOptions options)
        {
            if (name != "Umbraco." + SchemeName)
            {
                return;
            }

            Configure(options);
        }

        public void Configure(BackOfficeExternalLoginProviderOptions options)
        {
            options.AutoLinkOptions = new ExternalSignInAutoLinkOptions(
                // must be true for auto-linking to be enabled
                // Optionally specify the default culture to create
                // the user as. If null it will use the default
                // culture defined in the web.config, or it can
                // be dynamically assigned in the OnAutoLinking
                // callback.
            )
            {
                // Optional callback
                OnAutoLinking = (autoLinkUser, loginInfo) =>
                {
                    // You can customize the user before it's linked.
                    // i.e. Modify the user's groups based on the Claims returned
                    // in the externalLogin info
                },
                OnExternalLogin = (user, loginInfo) =>
                {
                    // You can customize the user before it's saved whenever they have
                    // logged in with the external provider.
                    // i.e. Sync the user's name based on the Claims returned
                    // in the externalLogin info

                    return true; //returns a boolean indicating if sign in should continue or not.
                }
            };

            // Optionally choose to automatically redirect to the
            // external login provider so the user doesn't have
            // to click the login button. This is
            options.AutoRedirectLoginToExternalProvider = false;
        }
    }
}
