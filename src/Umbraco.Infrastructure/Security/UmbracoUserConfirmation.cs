using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Confirms whether a user is approved or not
/// </summary>
public class UmbracoUserConfirmation<TUser> : DefaultUserConfirmation<TUser>
    where TUser : UmbracoIdentityUser
{
    /// <summary>
    /// Determines asynchronously whether the specified user has been confirmed, based on the user's <c>IsApproved</c> property.
    /// </summary>
    /// <param name="manager">The user manager instance.</param>
    /// <param name="user">The user whose confirmation status is being checked.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <c>true</c> if the user's <c>IsApproved</c> property is <c>true</c>; otherwise, <c>false</c>.</returns>
    public override Task<bool> IsConfirmedAsync(UserManager<TUser> manager, TUser user)
        => Task.FromResult(user.IsApproved);
}
