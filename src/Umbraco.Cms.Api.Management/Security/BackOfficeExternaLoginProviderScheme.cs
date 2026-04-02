using Microsoft.AspNetCore.Authentication;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
/// Represents a configuration scheme for an external login provider used in the Umbraco back office.
/// </summary>
public class BackOfficeExternaLoginProviderScheme
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeExternaLoginProviderScheme"/> class with the specified external login provider and optional authentication scheme.
    /// </summary>
    /// <param name="externalLoginProvider">The <see cref="BackOfficeExternalLoginProvider"/> instance representing the external login provider.</param>
    /// <param name="authenticationScheme">An optional <see cref="AuthenticationScheme"/> to use for authentication; can be <c>null</c>.</param>
    public BackOfficeExternaLoginProviderScheme(
        BackOfficeExternalLoginProvider externalLoginProvider,
        AuthenticationScheme? authenticationScheme)
    {
        ExternalLoginProvider = externalLoginProvider ?? throw new ArgumentNullException(nameof(externalLoginProvider));
        AuthenticationScheme = authenticationScheme ?? throw new ArgumentNullException(nameof(authenticationScheme));
    }

    /// <summary>
    /// Gets the external login provider associated with this back office authentication scheme.
    /// </summary>
    public BackOfficeExternalLoginProvider ExternalLoginProvider { get; }
    /// <summary>
    /// Gets the authentication scheme for the external login provider used in the back office.
    /// </summary>
    public AuthenticationScheme AuthenticationScheme { get; }
}
