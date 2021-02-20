using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Infrastructure.Security;
using Umbraco.Web.Models;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Changes the password for an identity user
    /// </summary>
    internal class PasswordChanger<TUser> : IPasswordChanger<TUser>
        where TUser : UmbracoIdentityUser
    {
        private readonly ILogger<PasswordChanger<TUser>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordChanger"/> class.
        /// Password changing functionality
        /// </summary>
        /// <param name="logger">Logger for this class</param>
        public PasswordChanger(ILogger<PasswordChanger<TUser>> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Changes the password for a user based on the many different rules and config options
        /// </summary>
        /// <param name="changingPasswordModel">The changing password model</param>
        /// <param name="userMgr">The identity manager to use to update the password</param>
        /// Create an adapter to pass through everything - adapting the member into a user for this functionality
        /// <returns>The outcome of the password changed model</returns>
        public async Task<Attempt<PasswordChangedModel>> ChangePasswordWithIdentityAsync(
            ChangingPasswordModel changingPasswordModel,
            IUmbracoUserManager<TUser> userMgr)
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
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Cannot set an empty password", new[] { "value" }) });
            }

            TUser identityUser = await userMgr.FindByIdAsync(changingPasswordModel.SavingUserId.ToString());
            if (identityUser == null)
            {
                // this really shouldn't ever happen... but just in case
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password could not be verified", new[] { "oldPassword" }) });
            }

            // Are we just changing another user's password?
            if (changingPasswordModel.OldPassword.IsNullOrWhiteSpace())
            {
                // if it's the current user, the current user cannot reset their own password
                // For members, this should not happen
                if (changingPasswordModel.CurrentUsername == changingPasswordModel.SavingUsername)
                {
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password reset is not allowed", new[] { "value" }) });
                }

                // if the current user has access to reset/manually change the password
                if (changingPasswordModel.CurrentUserHasSectionAccess)
                {
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("The current user is not authorized", new[] { "value" }) });
                }

                // ok, we should be able to reset it
                string resetToken = await userMgr.GeneratePasswordResetTokenAsync(identityUser);

                IdentityResult resetResult = await userMgr.ChangePasswordWithResetAsync(changingPasswordModel.SavingUserId.ToString(), resetToken, changingPasswordModel.NewPassword);

                if (resetResult.Succeeded == false)
                {
                    string errors = resetResult.Errors.ToErrorMessage();
                    _logger.LogWarning("Could not reset user password {PasswordErrors}", errors);
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult(errors, new[] { "value" }) });
                }

                return Attempt.Succeed(new PasswordChangedModel());
            }

            // is the old password correct?
            bool validateResult = await userMgr.CheckPasswordAsync(identityUser, changingPasswordModel.OldPassword);
            if (validateResult == false)
            {
                // no, fail with an error message for "oldPassword"
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Incorrect password", new[] { "oldPassword" }) });
            }

            // can we change to the new password?
            IdentityResult changeResult = await userMgr.ChangePasswordAsync(identityUser, changingPasswordModel.OldPassword, changingPasswordModel.NewPassword);
            if (changeResult.Succeeded == false)
            {
                // no, fail with error messages for "password"
                string errors = changeResult.Errors.ToErrorMessage();
                _logger.LogWarning("Could not change user password {PasswordErrors}", errors);
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult(errors, new[] { "password" }) });
            }

            return Attempt.Succeed(new PasswordChangedModel());
        }
    }
}
