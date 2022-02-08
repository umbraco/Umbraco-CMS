using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.UI.Security
{
    public class GoogleMemberExternalLoginProviderOptions : IConfigureNamedOptions<MemberExternalLoginProviderOptions>
    {
        public const string SchemeName = "GoogleMembers";

        public void Configure(string name, MemberExternalLoginProviderOptions options)
        {
            if (name != Constants.Security.MemberExternalAuthenticationTypePrefix + SchemeName)
            {
                return;
            }

            Configure(options);
        }

        public void Configure(MemberExternalLoginProviderOptions options) =>
            options.AutoLinkOptions = new MemberExternalSignInAutoLinkOptions(
                // must be true for auto-linking to be enabled
                true,

                // Optionally specify the default culture to create
                // the user as. If null it will use the default
                // culture defined in the web.config, or it can
                // be dynamically assigned in the OnAutoLinking
                // callback.
                defaultCulture: null
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
    }
}
