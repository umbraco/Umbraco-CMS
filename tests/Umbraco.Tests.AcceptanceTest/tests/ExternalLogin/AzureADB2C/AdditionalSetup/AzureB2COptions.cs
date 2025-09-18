using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Security;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.AcceptanceTest.ExternalLogin.AzureADB2C
{
    public class AzureB2COptions : IConfigureNamedOptions<BackOfficeExternalLoginProviderOptions>
    {
        public const string SchemeName = "AzureB2C";

        public void Configure(string? name, BackOfficeExternalLoginProviderOptions options)
        {
            if (name != Constants.Security.BackOfficeExternalAuthenticationTypePrefix + SchemeName)
                return;

            options.AutoLinkOptions = new ExternalSignInAutoLinkOptions(
                autoLinkExternalAccount: true,
                defaultUserGroups: [Constants.Security.AdminGroupAlias],
                defaultCulture: "en-US",
                allowManualLinking: true
            )
            {
                OnAutoLinking = (user, loginInfo) => { user.IsApproved = true; },
                OnExternalLogin = (user, loginInfo) => { return true; }
            };
        }

        public void Configure(BackOfficeExternalLoginProviderOptions options) =>
            Configure(Constants.Security.BackOfficeExternalAuthenticationTypePrefix + SchemeName, options);
    }
}
