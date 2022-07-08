using Microsoft.AspNetCore.Authentication;

namespace Umbraco.Cms.Web.Common.Security;

public class MemberExternalLoginProviderScheme
{
    public MemberExternalLoginProviderScheme(
        MemberExternalLoginProvider externalLoginProvider,
        AuthenticationScheme? authenticationScheme)
    {
        ExternalLoginProvider = externalLoginProvider ?? throw new ArgumentNullException(nameof(externalLoginProvider));
        AuthenticationScheme = authenticationScheme ?? throw new ArgumentNullException(nameof(authenticationScheme));
    }

    public MemberExternalLoginProvider ExternalLoginProvider { get; }

    public AuthenticationScheme AuthenticationScheme { get; }
}
