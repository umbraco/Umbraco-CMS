using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Represents a service for changing the password of a user of type <typeparamref name="TUser"/>.
/// </summary>
public interface IPasswordChanger<TUser> where TUser : UmbracoIdentityUser
{
    /// <summary>
    /// Asynchronously changes the password for a user using the provided identity and password information.
    /// </summary>
    /// <param name="passwordModel">The model containing the current and new password details.</param>
    /// <param name="userMgr">The user manager used to perform user-related operations.</param>
    /// <param name="currentUser">The user performing the password change, or <c>null</c> if not applicable.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{PasswordChangedModel}"/> indicating the outcome of the password change.</returns>
    public Task<Attempt<PasswordChangedModel?>> ChangePasswordWithIdentityAsync(
        ChangingPasswordModel passwordModel,
        IUmbracoUserManager<TUser> userMgr,
        IUser? currentUser);
}
