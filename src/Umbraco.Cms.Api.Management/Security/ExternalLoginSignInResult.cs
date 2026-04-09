using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Result returned from signing in when external logins are used.
/// </summary>
public class ExternalLoginSignInResult : SignInResult
{
    /// <summary>
    ///     Gets a <see cref="ExternalLoginSignInResult" /> that represents a sign-in attempt that failed because
    ///     the user is not allowed to sign-in.
    /// </summary>
    /// <value>
    ///     A <see cref="ExternalLoginSignInResult" /> that represents a sign-in attempt that failed because
    ///     the user is not allowed to sign-in.
    /// </value>
    public static new ExternalLoginSignInResult NotAllowed { get; } = new() { Succeeded = false };
}
