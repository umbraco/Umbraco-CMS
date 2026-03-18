using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
/// Provides functionality to change the back office user's password.
/// </summary>
public class BackOfficePasswordChanger : IBackOfficePasswordChanger
{
    private readonly IPasswordChanger<BackOfficeIdentityUser> _passwordChanger;
    private readonly IBackOfficeUserManager _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficePasswordChanger"/> class.
    /// </summary>
    /// <param name="passwordChanger">An implementation for changing passwords of <see cref="BackOfficeIdentityUser"/> instances.</param>
    /// <param name="userManager">The user manager responsible for managing back office users.</param>
    public BackOfficePasswordChanger(
        IPasswordChanger<BackOfficeIdentityUser> passwordChanger,
        IBackOfficeUserManager userManager)
    {
        _passwordChanger = passwordChanger;
        _userManager = userManager;
    }

    /// <summary>
    /// Asynchronously changes the password for a specified back office user.
    /// </summary>
    /// <param name="model">A <see cref="ChangeBackOfficeUserPasswordModel"/> containing the user identifier and password change details (old password, new password, and optional reset token).</param>
    /// <param name="performingUser">The <see cref="IUser"/> instance representing the user performing the password change, or <c>null</c> if not applicable.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation. The result is an <see cref="Attempt{T}"/> containing a <see cref="PasswordChangedModel"/> if the password change was successful; otherwise, the attempt indicates failure.
    /// </returns>
    public async Task<Attempt<PasswordChangedModel?>> ChangeBackOfficePassword(
        ChangeBackOfficeUserPasswordModel model, IUser? performingUser)
    {
        var mappedModel = new ChangingPasswordModel
        {
            Id = model.User.Id,
            OldPassword = model.OldPassword,
            NewPassword = model.NewPassword,
            ResetPasswordToken = model.ResetPasswordToken,
        };

        return await _passwordChanger.ChangePasswordWithIdentityAsync(mappedModel, _userManager, performingUser);
    }
}
