using System.Security.Principal;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides core user management functionality for back office users.
/// </summary>
public interface ICoreBackOfficeUserManager
{
    /// <summary>
    ///     Creates a new back office user.
    /// </summary>
    /// <param name="createModel">The model containing the user creation details.</param>
    /// <returns>A task that resolves to the result of the user creation operation.</returns>
    Task<IdentityCreationResult> CreateAsync(UserCreateModel createModel);

    /// <summary>
    ///     Creates a user for an invite. This means that the password will not be populated.
    /// </summary>
    /// <param name="createModel">The model containing the user creation details.</param>
    /// <returns>A task that resolves to the result of the user creation operation.</returns>
    Task<IdentityCreationResult> CreateForInvite(UserCreateModel createModel);

    /// <summary>
    ///     Generates an email confirmation token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>An attempt containing the generated token or an error status.</returns>
    Task<Attempt<string, UserOperationStatus>> GenerateEmailConfirmationTokenAsync(IUser user);

    /// <summary>
    ///     Generates a password reset token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>An attempt containing the generated token or an error status.</returns>
    Task<Attempt<string, UserOperationStatus>> GeneratePasswordResetTokenAsync(IUser user);

    /// <summary>
    ///     Unlocks the specified user account.
    /// </summary>
    /// <param name="user">The user to unlock.</param>
    /// <returns>An attempt containing the unlock result or an error status.</returns>
    Task<Attempt<UserUnlockResult, UserOperationStatus>> UnlockUser(IUser user);

    /// <summary>
    ///     Gets all external logins associated with the specified user.
    /// </summary>
    /// <param name="user">The user to get logins for.</param>
    /// <returns>An attempt containing the collection of logins or an error status.</returns>
    Task<Attempt<ICollection<IIdentityUserLogin>, UserOperationStatus>> GetLoginsAsync(IUser user);

    /// <summary>
    ///     Validates whether the email confirmation token is valid for the specified user.
    /// </summary>
    /// <param name="user">The user to validate the token for.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns><c>true</c> if the token is valid; otherwise, <c>false</c>.</returns>
    Task<bool> IsEmailConfirmationTokenValidAsync(IUser user, string token);

    /// <summary>
    ///     Validates whether the password reset token is valid for the specified user.
    /// </summary>
    /// <param name="user">The user to validate the token for.</param>
    /// <param name="token">The token to validate.</param>
    /// <returns><c>true</c> if the token is valid; otherwise, <c>false</c>.</returns>
    Task<bool> IsResetPasswordTokenValidAsync(IUser user, string token);

    /// <summary>
    ///     Notifies that a forgot password request was made.
    /// </summary>
    /// <param name="user">The principal of the user who requested the password reset.</param>
    /// <param name="toString">Additional information about the request.</param>
    void NotifyForgotPasswordRequested(IPrincipal user, string toString);

    /// <summary>
    ///     Generates a random password that meets the configured password requirements.
    /// </summary>
    /// <returns>A randomly generated password.</returns>
    public string GeneratePassword();
}
