using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Result returned from signing in when auto-linking takes place
/// </summary>
public class AutoLinkSignInResult : SignInResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoLinkSignInResult"/> class with the specified errors.
    /// </summary>
    /// <param name="errors">A collection of error messages related to the auto-link sign-in process.</param>
    public AutoLinkSignInResult(IReadOnlyCollection<string> errors) =>
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Security.AutoLinkSignInResult"/> class with default values.
    /// </summary>
    public AutoLinkSignInResult()
    {
    }

    /// <summary>Gets a result indicating that the sign-in attempt failed because the account is not linked.</summary>
    public static AutoLinkSignInResult FailedNotLinked { get; } = new() { Succeeded = false };

    /// <summary>
    /// Gets a result indicating the auto-link sign-in failed due to no email being available.
    /// </summary>
    public static AutoLinkSignInResult FailedNoEmail { get; } = new() { Succeeded = false };

    /// <summary>
    /// Gets the collection of error messages related to the auto-link sign-in process.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();

    /// <summary>Creates a failed AutoLinkSignInResult with the specified error message.</summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>An AutoLinkSignInResult indicating failure with the provided error.</returns>
    public static AutoLinkSignInResult FailedException(string error) => new(new[] { error }) { Succeeded = false };

    /// <summary>
    /// Creates a failed AutoLinkSignInResult with the specified errors.
    /// </summary>
    /// <param name="errors">The collection of error messages describing why the user creation failed.</param>
    /// <returns>An AutoLinkSignInResult indicating failure with error details.</returns>
    public static AutoLinkSignInResult FailedCreatingUser(IReadOnlyCollection<string> errors) =>
        new(errors) { Succeeded = false };

    /// <summary>
    /// Creates an <see cref="AutoLinkSignInResult"/> representing a failed auto-link sign-in attempt, including the specified errors.
    /// </summary>
    /// <param name="errors">A collection of error messages describing why the linking failed.</param>
    /// <returns>An <see cref="AutoLinkSignInResult"/> indicating failure and containing error details.</returns>
    public static AutoLinkSignInResult FailedLinkingUser(IReadOnlyCollection<string> errors) =>
        new(errors) { Succeeded = false };
}
