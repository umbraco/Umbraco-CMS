namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Used by the BackOfficeUserManager to check the username/password which allows for developers to more easily
///     set the logic for this procedure.
/// </summary>
public interface IBackOfficeUserPasswordChecker
{
    /// <summary>
    ///     Checks a password for a user
    /// </summary>
    /// <remarks>
    ///     This will allow a developer to auto-link a local account which is required if the user queried doesn't exist
    ///     locally.
    ///     The user parameter will always contain the username, if the user doesn't exist locally, the other properties will
    ///     not be filled in.
    ///     A developer can then create a local account by filling in the properties and using UserManager.CreateAsync
    /// </remarks>
    Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password);
}
