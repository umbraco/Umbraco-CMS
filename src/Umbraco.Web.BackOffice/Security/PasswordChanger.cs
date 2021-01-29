using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using Umbraco.Infrastructure.Security;
using Umbraco.Web.Models;
using IUser = Umbraco.Core.Models.Membership.IUser;

namespace Umbraco.Web.BackOffice.Security
{
    internal class PasswordChanger
    {
        private readonly ILogger<PasswordChanger> _logger;

        public PasswordChanger(ILogger<PasswordChanger> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Changes the password for a user based on the many different rules and config options
        /// </summary>
        /// <param name="currentUser">The user performing the password save action</param>
        /// <param name="savingUser">The user who's password is being changed</param>
        /// <param name="passwordModel"></param>
        /// <param name="userMgr"></param>
        /// <returns></returns>
        public async Task<Attempt<PasswordChangedModel>> ChangePasswordWithIdentityAsync(
            IUser currentUser,
            IUser savingUser,
            ChangingPasswordModel passwordModel,
            IBackOfficeUserManager userMgr)
        {
            if (passwordModel == null) throw new ArgumentNullException(nameof(passwordModel));
            if (userMgr == null) throw new ArgumentNullException(nameof(userMgr));

            if (passwordModel.NewPassword.IsNullOrWhiteSpace())
            {
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Cannot set an empty password", new[] { "value" }) });
            }

            var backOfficeIdentityUser = await userMgr.FindByIdAsync(savingUser.Id.ToString());
            if (backOfficeIdentityUser == null)
            {
                //this really shouldn't ever happen... but just in case
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password could not be verified", new[] { "oldPassword" }) });
            }

            //Are we just changing another user's password?
            if (passwordModel.OldPassword.IsNullOrWhiteSpace())
            {
                //if it's the current user, the current user cannot reset their own password
                if (currentUser.Username == savingUser.Username)
                {
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password reset is not allowed", new[] { "value" }) });
                }

                //if the current user has access to reset/manually change the password
                if (currentUser.HasSectionAccess(Umbraco.Core.Constants.Applications.Users) == false)
                {
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("The current user is not authorized", new[] { "value" }) });
                }

                //ok, we should be able to reset it
                var resetToken = await userMgr.GeneratePasswordResetTokenAsync(backOfficeIdentityUser);

                var resetResult = await userMgr.ChangePasswordWithResetAsync(savingUser.Id.ToString(), resetToken, passwordModel.NewPassword);

                if (resetResult.Succeeded == false)
                {
                    var errors = resetResult.Errors.ToErrorMessage();
                    _logger.LogWarning("Could not reset user password {PasswordErrors}", errors);
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult(errors, new[] { "value" }) });
                }

                return Attempt.Succeed(new PasswordChangedModel());
            }

            //is the old password correct?
            var validateResult = await userMgr.CheckPasswordAsync(backOfficeIdentityUser, passwordModel.OldPassword);
            if (validateResult == false)
            {
                //no, fail with an error message for "oldPassword"
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Incorrect password", new[] { "oldPassword" }) });
            }
            //can we change to the new password?
            var changeResult = await userMgr.ChangePasswordAsync(backOfficeIdentityUser, passwordModel.OldPassword, passwordModel.NewPassword);
            if (changeResult.Succeeded == false)
            {
                //no, fail with error messages for "password"
                var errors = changeResult.Errors.ToErrorMessage();
                _logger.LogWarning("Could not change user password {PasswordErrors}", errors);
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult(errors, new[] { "password" }) });
            }
            return Attempt.Succeed(new PasswordChangedModel());
        }

    }
}
