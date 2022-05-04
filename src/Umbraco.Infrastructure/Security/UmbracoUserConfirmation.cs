using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Confirms whether a user is approved or not
/// </summary>
public class UmbracoUserConfirmation<TUser> : DefaultUserConfirmation<TUser>
    where TUser : UmbracoIdentityUser
{
    public override Task<bool> IsConfirmedAsync(UserManager<TUser> manager, TUser user)
        => Task.FromResult(user.IsApproved);
}
