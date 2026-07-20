using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides functionality to change back office user passwords.
/// </summary>
public interface IBackOfficePasswordChanger
{
    /// <summary>
    ///     Changes the password for a back office user.
    /// </summary>
    /// <param name="model">The password change model containing the user and new password details.</param>
    /// <param name="performingUser">The user performing the password change operation.</param>
    /// <returns>An <see cref="Attempt{T}" /> containing the result of the password change operation.</returns>
    Task<Attempt<PasswordChangedModel?>> ChangeBackOfficePassword(ChangeBackOfficeUserPasswordModel model, IUser? performingUser);
}
