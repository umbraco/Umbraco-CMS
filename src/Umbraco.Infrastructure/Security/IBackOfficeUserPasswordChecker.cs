namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Used by the BackOfficeUserManager to check the username/password which allows for developers to more easily
///     set the logic for this procedure.
/// </summary>
public interface IBackOfficeUserPasswordChecker
{
    /// <summary>
    ///     Checks whether the specified password is valid for the given back office user.
    /// </summary>
    /// <remarks>
    ///     This method allows a developer to auto-link a local account, which is required if the queried user does not exist
    ///     locally. The <paramref name="user"/> parameter will always contain the username; if the user does not exist locally,
    ///     the other properties will not be populated. A developer can then create a local account by filling in the properties
    ///     and using <c>UserManager.CreateAsync</c>.
    /// </remarks>
    /// <param name="user">The back office identity user whose password is being validated. The username will always be set; other properties may be null if the user does not exist locally.</param>
    /// <param name="password">The password to validate against the user's credentials.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the outcome of the password check.</returns>
    Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password);
}
