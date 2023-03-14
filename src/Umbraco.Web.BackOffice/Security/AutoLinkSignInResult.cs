using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Result returned from signing in when auto-linking takes place
/// </summary>
public class AutoLinkSignInResult : SignInResult
{
    public AutoLinkSignInResult(IReadOnlyCollection<string> errors) =>
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));

    public AutoLinkSignInResult()
    {
    }

    public static AutoLinkSignInResult FailedNotLinked { get; } = new() { Succeeded = false };

    public static AutoLinkSignInResult FailedNoEmail { get; } = new() { Succeeded = false };

    public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();

    public static AutoLinkSignInResult FailedException(string error) => new(new[] { error }) { Succeeded = false };

    public static AutoLinkSignInResult FailedCreatingUser(IReadOnlyCollection<string> errors) =>
        new(errors) { Succeeded = false };

    public static AutoLinkSignInResult FailedLinkingUser(IReadOnlyCollection<string> errors) =>
        new(errors) { Succeeded = false };
}
