using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Result returned from signing in when external logins are used.
/// </summary>
public class ExternalLoginSignInResult : SignInResult
{
    public static ExternalLoginSignInResult NotAllowed { get; } = new() { Succeeded = false };
}
