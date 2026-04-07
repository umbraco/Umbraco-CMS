namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// Represents the result of a successful sign-out operation, typically used to indicate that a user has been signed out in an authentication or security context.
/// </summary>
public class SignOutSuccessResult
{
    /// <summary>
    /// Gets or sets the URL to redirect to after a successful sign-out.
    /// </summary>
    public string? SignOutRedirectUrl { get; set; }
}
