using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A user manager for Umbraco (either back office users or front-end members)
/// </summary>
/// <typeparam name="TUser">The type of user</typeparam>
public interface IUmbracoUserManager<TUser> : IDisposable
    where TUser : UmbracoIdentityUser
{
    /// <summary>
    ///     Gets the user id of a user
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    Task<string> GetUserIdAsync(TUser user);

    /// <summary>
    ///     Get the <see cref="TUser" /> from a <see cref="ClaimsPrincipal" />
    /// </summary>
    /// <param name="principal">The <see cref="ClaimsPrincipal" /></param>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    Task<TUser> GetUserAsync(ClaimsPrincipal principal);

    /// <summary>
    ///     Get the user id from the <see cref="ClaimsPrincipal" />
    /// </summary>
    /// <param name="principal">the <see cref="ClaimsPrincipal" /></param>
    /// <returns>Returns the user id from the <see cref="ClaimsPrincipal" /></returns>
    string GetUserId(ClaimsPrincipal principal);

    /// <summary>
    ///     Gets the external logins for the user
    /// </summary>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user);

    /// <summary>
    ///     Deletes a user
    /// </summary>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    Task<IdentityResult> DeleteAsync(TUser user);

    /// <summary>
    ///     Finds a user by the external login provider
    /// </summary>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    Task<TUser> FindByLoginAsync(string loginProvider, string providerKey);

    /// <summary>
    ///     Finds and returns a user, if any, who has the specified <paramref name="userId" />.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the user matching the specified
    ///     <paramref name="userId" /> if it exists.
    /// </returns>
    Task<TUser> FindByIdAsync(string? userId);

    /// <summary>
    ///     Generates a password reset token for the specified <paramref name="user" />, using
    ///     the configured password reset token provider.
    /// </summary>
    /// <param name="user">The user to generate a password reset token for.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation,
    ///     containing a password reset token for the specified <paramref name="user" />.
    /// </returns>
    Task<string> GeneratePasswordResetTokenAsync(TUser user);

    /// <summary>
    ///     This is a special method that will reset the password but will raise the Password Changed event instead of the
    ///     reset event
    /// </summary>
    /// <remarks>
    ///     We use this because in the back office the only way an admin can change another user's password without first
    ///     knowing their password
    ///     is to generate a token and reset it, however, when we do this we want to track a password change, not a password
    ///     reset
    /// </remarks>
    Task<IdentityResult> ChangePasswordWithResetAsync(string userId, string token, string? newPassword);

    /// <summary>
    ///     Validates that an email confirmation token matches the specified <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user to validate the token against.</param>
    /// <param name="token">The email confirmation token to validate.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the <see cref="IdentityResult" />
    ///     of the operation.
    /// </returns>
    Task<IdentityResult> ConfirmEmailAsync(TUser user, string? token);

    /// <summary>
    ///     Gets the user, if any, associated with the normalized value of the specified email address.
    ///     Note: Its recommended that identityOptions.User.RequireUniqueEmail be set to true when using this method, otherwise
    ///     the store may throw if there are users with duplicate emails.
    /// </summary>
    /// <param name="email">The email address to return the user for.</param>
    /// <returns>
    ///     The task object containing the results of the asynchronous lookup operation, the user, if any, associated with a
    ///     normalized value of the specified email address.
    /// </returns>
    Task<TUser> FindByEmailAsync(string email);

    /// <summary>
    ///     Resets the <paramref name="user" />'s password to the specified <paramref name="newPassword" /> after
    ///     validating the given password reset <paramref name="token" />.
    /// </summary>
    /// <param name="user">The user whose password should be reset.</param>
    /// <param name="token">The password reset token to verify.</param>
    /// <param name="newPassword">The new password to set if reset token verification succeeds.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the <see cref="IdentityResult" />
    ///     of the operation.
    /// </returns>
    Task<IdentityResult> ResetPasswordAsync(TUser user, string? token, string? newPassword);

    /// <summary>
    ///     Override to check the user approval value as well as the user lock out date, by default this only checks the user's
    ///     locked out date
    /// </summary>
    /// <remarks>
    ///     In the ASP.NET Identity world, there is only one value for being locked out, in Umbraco we have 2 so when checking
    ///     this for Umbraco we need to check both values
    /// </remarks>
    Task<bool> IsLockedOutAsync(TUser user);

    /// <summary>
    ///     Locks out a user until the specified end date has passed. Setting a end date in the past immediately unlocks a
    ///     user.
    /// </summary>
    /// <param name="user">The user whose lockout date should be set.</param>
    /// <param name="lockoutEnd">
    ///     The <see cref="DateTimeOffset" /> after which the <paramref name="user" />'s lockout should
    ///     end.
    /// </param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the
    ///     <see cref="IdentityResult" /> of the operation.
    /// </returns>
    Task<IdentityResult> SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd);

    /// <summary>
    ///     Gets a flag indicating whether the email address for the specified <paramref name="user" /> has been verified, true
    ///     if the email address is verified otherwise
    ///     false.
    /// </summary>
    /// <param name="user">The user whose email confirmation status should be returned.</param>
    /// <returns>
    ///     The task object containing the results of the asynchronous operation, a flag indicating whether the email address
    ///     for the specified <paramref name="user" />
    ///     has been confirmed or not.
    /// </returns>
    Task<bool> IsEmailConfirmedAsync(TUser user);

    /// <summary>
    ///     Updates the specified <paramref name="user" /> in the backing store.
    /// </summary>
    /// <param name="user">The user to update.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the <see cref="IdentityResult" />
    ///     of the operation.
    /// </returns>
    Task<IdentityResult> UpdateAsync(TUser user);

    /// <summary>
    ///     Returns a flag indicating whether the specified <paramref name="token" /> is valid for
    ///     the given <paramref name="user" /> and <paramref name="purpose" />.
    /// </summary>
    /// <param name="user">The user to validate the token against.</param>
    /// <param name="tokenProvider">The token provider used to generate the token.</param>
    /// <param name="purpose">The purpose the token should be generated for.</param>
    /// <param name="token">The token to validate</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, returning true if the <paramref name="token" />
    ///     is valid, otherwise false.
    /// </returns>
    Task<bool> VerifyUserTokenAsync(TUser user, string tokenProvider, string purpose, string token);

    /// <summary>
    ///     Adds the <paramref name="password" /> to the specified <paramref name="user" /> only if the user
    ///     does not already have a password.
    /// </summary>
    /// <param name="user">The user whose password should be set.</param>
    /// <param name="password">The password to set.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the <see cref="IdentityResult" />
    ///     of the operation.
    /// </returns>
    Task<IdentityResult> AddPasswordAsync(TUser user, string password);

    /// <summary>
    ///     Returns a flag indicating whether the given <paramref name="password" /> is valid for the
    ///     specified <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user whose password should be validated.</param>
    /// <param name="password">The password to validate</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing true if
    ///     the specified <paramref name="password" /> matches the one store for the <paramref name="user" />,
    ///     otherwise false.
    /// </returns>
    Task<bool> CheckPasswordAsync(TUser user, string? password);

    /// <summary>
    ///     Changes a user's password after confirming the specified <paramref name="currentPassword" /> is correct,
    ///     as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose password should be set.</param>
    /// <param name="currentPassword">The current password to validate before changing.</param>
    /// <param name="newPassword">The new password to set for the specified <paramref name="user" />.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the <see cref="IdentityResult" />
    ///     of the operation.
    /// </returns>
    Task<IdentityResult> ChangePasswordAsync(TUser user, string? currentPassword, string? newPassword);

    /// <summary>
    ///     Used to validate a user's session
    /// </summary>
    /// <returns>Returns true if the session is valid, otherwise false</returns>
    Task<bool> ValidateSessionIdAsync(string? userId, string? sessionId);

    /// <summary>
    ///     Creates the specified <paramref name="user" /> in the backing store with no password,
    ///     as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the <see cref="IdentityResult" />
    ///     of the operation.
    /// </returns>
    Task<IdentityResult> CreateAsync(TUser user);

    /// <summary>
    ///     Gets a list of role names the specified user belongs to.
    /// </summary>
    /// <param name="user">The user whose role names to retrieve.</param>
    /// <returns>The Task that represents the asynchronous operation, containing a list of role names.</returns>
    Task<IList<string>> GetRolesAsync(TUser user);

    /// <summary>
    ///     Removes the specified user from the named roles.
    /// </summary>
    /// <param name="user">The user to remove from the named roles.</param>
    /// <param name="roles">The name of the roles to remove the user from.</param>
    /// <returns>The Task that represents the asynchronous operation, containing the IdentityResult of the operation.</returns>
    Task<IdentityResult> RemoveFromRolesAsync(TUser user, IEnumerable<string> roles);

    /// <summary>
    ///     Add the specified user to the named roles
    /// </summary>
    /// <param name="user">The user to add to the named roles</param>
    /// <param name="roles">The name of the roles to add the user to.</param>
    /// <returns>The Task that represents the asynchronous operation, containing the IdentityResult of the operation</returns>
    Task<IdentityResult> AddToRolesAsync(TUser user, IEnumerable<string> roles);

    /// <summary>
    ///     Creates the specified <paramref name="user" /> in the backing store with a password,
    ///     as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="password">The password to add to the user.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the <see cref="IdentityResult" />
    ///     of the operation.
    /// </returns>
    Task<IdentityResult> CreateAsync(TUser user, string? password);

    /// <summary>
    ///     Generate a password for a user based on the current password validator
    /// </summary>
    /// <returns>A generated password</returns>
    string GeneratePassword();

    /// <summary>
    ///     Used to validate the password without an identity user
    ///     Validation code is based on the default ValidatePasswordAsync code
    ///     Should return <see cref="IdentityResult.Success" /> if validation is successful
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>A <see cref="IdentityResult" /> representing whether validation was successful.</returns>
    Task<IdentityResult> ValidatePasswordAsync(string? password);

    /// <summary>
    ///     Generates an email confirmation token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate an email confirmation token for.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, an email confirmation token.
    /// </returns>
    Task<string> GenerateEmailConfirmationTokenAsync(TUser user);

    /// <summary>
    ///     Finds and returns a user, if any, who has the specified user name.
    /// </summary>
    /// <param name="userName">The user name to search for.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the user matching the specified
    ///     <paramref name="userName" /> if it exists.
    /// </returns>
    Task<TUser> FindByNameAsync(string userName);

    /// <summary>
    ///     Increments the access failed count for the user as an asynchronous operation.
    ///     If the failed access account is greater than or equal to the configured maximum number of attempts,
    ///     the user will be locked out for the configured lockout time span.
    /// </summary>
    /// <param name="user">The user whose failed access count to increment.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, containing the
    ///     <see cref="IdentityResult" /> of the operation.
    /// </returns>
    Task<IdentityResult> AccessFailedAsync(TUser user);

    /// <summary>
    ///     Returns a flag indicating whether the specified <paramref name="user" /> has two factor authentication enabled or
    ///     not,
    ///     as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user whose two factor authentication enabled status should be retrieved.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents the asynchronous operation, true if the specified <paramref name="user " />
    ///     has two factor authentication enabled, otherwise false.
    /// </returns>
    Task<bool> GetTwoFactorEnabledAsync(TUser user);

    /// <summary>
    ///     Gets a list of valid two factor token providers for the specified <paramref name="user" />,
    ///     as an asynchronous operation.
    /// </summary>
    /// <param name="user">The user the whose two factor authentication providers will be returned.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents result of the asynchronous operation, a list of two
    ///     factor authentication providers for the specified user.
    /// </returns>
    Task<IList<string>> GetValidTwoFactorProvidersAsync(TUser user);

    /// <summary>
    ///     Verifies the specified two factor authentication <paramref name="token" /> against the <paramref name="user" />.
    /// </summary>
    /// <param name="user">The user the token is supposed to be for.</param>
    /// <param name="tokenProvider">The provider which will verify the token.</param>
    /// <param name="token">The token to verify.</param>
    /// <returns>
    ///     The <see cref="Task" /> that represents result of the asynchronous operation, true if the token is valid,
    ///     otherwise false.
    /// </returns>
    Task<bool> VerifyTwoFactorTokenAsync(TUser user, string tokenProvider, string token);

    /// <summary>
    ///     Adds an external Microsoft.AspNetCore.Identity.UserLoginInfo to the specified user.
    /// </summary>
    /// <param name="user">The user to add the login to.</param>
    /// <param name="login">The external Microsoft.AspNetCore.Identity.UserLoginInfo to add to the specified user.</param>
    /// <returns>
    ///     The System.Threading.Tasks.Task that represents the asynchronous operation, containing the
    ///     Microsoft.AspNetCore.Identity.IdentityResult of the operation.
    /// </returns>
    Task<IdentityResult> AddLoginAsync(TUser user, UserLoginInfo login);

    /// <summary>
    ///     Attempts to remove the provided external login information from the specified user. and returns a flag indicating
    ///     whether the removal succeed or not.
    /// </summary>
    /// <param name="user">The user to remove the login information from.</param>
    /// <param name="loginProvider">The login provide whose information should be removed.</param>
    /// <param name="providerKey">The key given by the external login provider for the specified user.</param>
    /// <returns>
    ///     The System.Threading.Tasks.Task that represents the asynchronous operation, containing the
    ///     Microsoft.AspNetCore.Identity.IdentityResult of the operation.
    /// </returns>
    Task<IdentityResult> RemoveLoginAsync(TUser user, string? loginProvider, string? providerKey);

    /// <summary>
    ///     Resets the access failed count for the user
    /// </summary>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    Task<IdentityResult> ResetAccessFailedCountAsync(TUser user);

    /// <summary>
    ///     Generates a two factor token for the user
    /// </summary>
    /// <returns>A <see cref="Task{TResult}" /> representing the result of the asynchronous operation.</returns>
    Task<string> GenerateTwoFactorTokenAsync(TUser user, string tokenProvider);

    /// <summary>
    ///     Gets the email address for the specified user.
    /// </summary>
    /// <param name="user">The user whose email should be returned.</param>
    /// <returns>
    ///     The task object containing the results of the asynchronous operation, the email address for the specified
    ///     user.
    /// </returns>
    Task<string> GetEmailAsync(TUser user);

    /// <summary>
    ///     Gets the telephone number, if any, for the specified user.
    /// </summary>
    /// <param name="user">The user whose telephone number should be retrieved.</param>
    /// <returns>
    ///     The System.Threading.Tasks.Task that represents the asynchronous operation, containing the user's telephone
    ///     number, if any.
    /// </returns>
    /// <remarks>
    ///     A user can only support a phone number if the BackOfficeUserStore is replaced with another that implements
    ///     IUserPhoneNumberStore
    /// </remarks>
    Task<string> GetPhoneNumberAsync(TUser user);

    /// <summary>
    ///     Validates that a user's credentials are correct without actually logging them in.
    /// </summary>
    /// <param name="username">The user name.</param>
    /// <param name="password">The password.</param>
    /// <returns>True if the credentials are valid.</returns>
    Task<bool> ValidateCredentialsAsync(string username, string password);
}
