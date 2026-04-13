using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Provides validation logic for back office users of type <typeparamref name="T"/>.
/// </summary>
public class BackOfficeUserValidator<T> : UserValidator<T>
    where T : BackOfficeIdentityUser
{
    /// <summary>
    /// Asynchronously validates the specified user, only performing validation if the user's email or username has changed.
    /// </summary>
    /// <param name="manager">The user manager instance used for validation.</param>
    /// <param name="user">The user to validate.</param>
    /// <returns>A task representing the asynchronous validation operation. The task result contains the <see cref="IdentityResult"/> of the validation.</returns>
    public override async Task<IdentityResult> ValidateAsync(UserManager<T> manager, T user)
    {
        // Don't validate if the user's email or username hasn't changed otherwise it's just wasting SQL queries.
        if (user.IsPropertyDirty("Email") || user.IsPropertyDirty("UserName"))
        {
            return await base.ValidateAsync(manager, user);
        }

        return IdentityResult.Success;
    }
}
