using Microsoft.AspNetCore.Authentication;

namespace Umbraco.Cms.Api.Management.Security;

public class BackOfficeExternaLoginProviderScheme
{
    public BackOfficeExternaLoginProviderScheme(
        BackOfficeExternalLoginProvider externalLoginProvider,
        AuthenticationScheme? authenticationScheme)
    {
        ExternalLoginProvider = externalLoginProvider ?? throw new ArgumentNullException(nameof(externalLoginProvider));
        AuthenticationScheme = authenticationScheme ?? throw new ArgumentNullException(nameof(authenticationScheme));
    }

    public BackOfficeExternalLoginProvider ExternalLoginProvider { get; }
    public AuthenticationScheme AuthenticationScheme { get; }
}
