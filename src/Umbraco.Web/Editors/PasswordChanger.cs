using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Security;
using IUser = Umbraco.Core.Models.Membership.IUser;

namespace Umbraco.Web.Editors
{
    internal class PasswordChanger
    {
        private readonly ILogger _logger;

        public PasswordChanger(ILogger logger)
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
            BackOfficeUserManager<BackOfficeIdentityUser> userMgr)
        {
            if (passwordModel == null) throw new ArgumentNullException(nameof(passwordModel));
            if (userMgr == null) throw new ArgumentNullException(nameof(userMgr));

            if (passwordModel.NewPassword.IsNullOrWhiteSpace())
            {
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Cannot set an empty password", new[] { "value" }) });
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
                var resetToken = await userMgr.GeneratePasswordResetTokenAsync(savingUser.Id);

                var resetResult = await userMgr.ChangePasswordWithResetAsync(savingUser.Id, resetToken, passwordModel.NewPassword);

                if (resetResult.Succeeded == false)
                {
                    var errors = string.Join(". ", resetResult.Errors);
                    _logger.Warn<PasswordChanger>("Could not reset user password {PasswordErrors}", errors);
                    return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult(errors, new[] { "value" }) });
                }

                return Attempt.Succeed(new PasswordChangedModel());
            }

            //we're changing our own password...

            //get the user
            var backOfficeIdentityUser = await userMgr.FindByIdAsync(savingUser.Id);
            if (backOfficeIdentityUser == null)
            {
                //this really shouldn't ever happen... but just in case
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password could not be verified", new[] { "oldPassword" }) });
            }
            //is the old password correct?
            var validateResult = await userMgr.CheckPasswordAsync(backOfficeIdentityUser, passwordModel.OldPassword);
            if (validateResult == false)
            {
                //no, fail with an error message for "oldPassword"
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Incorrect password", new[] { "oldPassword" }) });
            }
            //can we change to the new password?
            var changeResult = await userMgr.ChangePasswordAsync(savingUser.Id, passwordModel.OldPassword, passwordModel.NewPassword);
            if (changeResult.Succeeded == false)
            {
                //no, fail with error messages for "password"
                var errors = string.Join(". ", changeResult.Errors);
                _logger.Warn<PasswordChanger>("Could not change user password {PasswordErrors}", errors);
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult(errors, new[] { "password" }) });
            }
            return Attempt.Succeed(new PasswordChangedModel());
        }

    }
}
