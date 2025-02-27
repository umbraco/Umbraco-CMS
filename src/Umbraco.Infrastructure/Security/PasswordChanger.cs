using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Changes the password for an identity user
/// </summary>
internal class PasswordChanger<TUser> : IPasswordChanger<TUser> where TUser : UmbracoIdentityUser
{
    private readonly ILogger<PasswordChanger<TUser>> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PasswordChanger{TUser}"/> class.
    ///     Password changing functionality
    /// </summary>
    /// <param name="logger">Logger for this class</param>
    public PasswordChanger(ILogger<PasswordChanger<TUser>> logger) => _logger = logger;

    public Task<Attempt<PasswordChangedModel?>> ChangePasswordWithIdentityAsync(ChangingPasswordModel passwordModel, IUmbracoUserManager<TUser> userMgr) => ChangePasswordWithIdentityAsync(passwordModel, userMgr, null);

    /// <summary>
    ///     Changes the password for a user based on the many different rules and config options
    /// </summary>
    /// <param name="changingPasswordModel">The changing password model.</param>
    /// <param name="userMgr">The identity manager to use to update the password.</param>
    /// <param name="currentUser">The user performing the operation.</param>
    /// Create an adapter to pass through everything - adapting the member into a user for this functionality
    /// <returns>The outcome of the password changed model</returns>
    public async Task<Attempt<PasswordChangedModel?>> ChangePasswordWithIdentityAsync(
        ChangingPasswordModel changingPasswordModel,
        IUmbracoUserManager<TUser> userMgr,
        IUser? currentUser)
    {
        if (changingPasswordModel == null)
        {
            throw new ArgumentNullException(nameof(changingPasswordModel));
        }

        if (userMgr == null)
        {
            throw new ArgumentNullException(nameof(userMgr));
        }

        if (changingPasswordModel.NewPassword.IsNullOrWhiteSpace())
        {
            return Attempt.Fail(new PasswordChangedModel
            {
                Error = new ValidationResult("Cannot set an empty password", new[] { "value" })
            });
        }

        var userId = changingPasswordModel.Id.ToString();
        TUser? identityUser = await userMgr.FindByIdAsync(userId);
        if (identityUser == null)
        {
            // this really shouldn't ever happen... but just in case
            return Attempt.Fail(new PasswordChangedModel
            {
                Error = new ValidationResult("Password could not be verified", new[] { "oldPassword" })
            });
        }

        // If old password is not specified we either have to change another user's password, or provide a reset password token
        if (changingPasswordModel.OldPassword.IsNullOrWhiteSpace())
        {
            if (changingPasswordModel.Id == currentUser?.Id && changingPasswordModel.ResetPasswordToken is null && currentUser.UserState != UserState.Invited)
            {
                return Attempt.Fail(new PasswordChangedModel
                {

                    Error = new ValidationResult("Cannot change the password of current user without the old password or a reset password token", new[] { "value" }),
                });
            }

            // ok, we should be able to reset it
            IdentityResult resetResult = changingPasswordModel.ResetPasswordToken is not null
                ? await userMgr.ResetPasswordAsync(identityUser, changingPasswordModel.ResetPasswordToken.FromUrlBase64()!, changingPasswordModel.NewPassword)
                : await userMgr.ChangePasswordWithResetAsync(userId, await userMgr.GeneratePasswordResetTokenAsync(identityUser), changingPasswordModel.NewPassword);

            if (resetResult.Succeeded == false)
            {
                var errors = resetResult.Errors.ToErrorMessage();
                _logger.LogWarning("Could not reset user password {PasswordErrors}", errors);
                return Attempt.Fail(new PasswordChangedModel
                {
                    Error = new ValidationResult(errors, new[] { "value" })
                });
            }

            return Attempt.Succeed(new PasswordChangedModel());
        }

        // is the old password correct?
        var validateResult = await userMgr.CheckPasswordAsync(identityUser, changingPasswordModel.OldPassword);
        if (validateResult == false)
        {
            // no, fail with an error message for "oldPassword"
            return Attempt.Fail(new PasswordChangedModel
            {
                Error = new ValidationResult("Incorrect password", new[] { "oldPassword" })
            });
        }

        // can we change to the new password?
        IdentityResult changeResult = await userMgr.ChangePasswordAsync(identityUser, changingPasswordModel.OldPassword!, changingPasswordModel.NewPassword);
        if (changeResult.Succeeded == false)
        {
            // no, fail with error messages for "password"
            var errors = changeResult.Errors.ToErrorMessage();
            _logger.LogWarning("Could not change user password {PasswordErrors}", errors);
            return Attempt.Fail(new PasswordChangedModel
            {
                Error = new ValidationResult(errors, new[] { "password" })
            });
        }

        return Attempt.Succeed(new PasswordChangedModel());
    }
}
