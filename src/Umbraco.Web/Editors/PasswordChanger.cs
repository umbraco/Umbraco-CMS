using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
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

        public string ChangePassword(ChangingPasswordModel passwordModel, PasswordSecurity passwordSecurity)
        {
            if (passwordModel.NewPassword.IsNullOrWhiteSpace())
                throw new InvalidOperationException("No password value");

            return passwordSecurity.HashPasswordForStorage(passwordModel.NewPassword);
        }

        /// <summary>
        /// Changes password for a member/user given the membership provider and the password change model
        /// </summary>
        /// <param name="username">The username of the user having their password changed</param>
        /// <param name="passwordModel"></param>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public Attempt<PasswordChangedModel> ChangePasswordWithMembershipProvider(
            string username,
            ChangingPasswordModel passwordModel,
            MembershipProvider membershipProvider)
        {
            var umbracoBaseProvider = membershipProvider as MembershipProviderBase;

            // YES! It is completely insane how many options you have to take into account based on the membership provider. yikes!

            if (passwordModel == null) throw new ArgumentNullException(nameof(passwordModel));
            if (membershipProvider == null) throw new ArgumentNullException(nameof(membershipProvider));           
            var userId = -1;


            //we're not resetting it so we need to try to change it.

            if (passwordModel.NewPassword.IsNullOrWhiteSpace())
            {
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Cannot set an empty password", new[] { "value" }) });
            }

            if (membershipProvider.EnablePasswordRetrieval)
            {
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Membership providers using encrypted passwords and password retrieval are not supported", new[] { "value" }) });
            }

            //without being able to retrieve the original password
            if (passwordModel.OldPassword.IsNullOrWhiteSpace())
            {
                //if password retrieval is not enabled but there is no old password we cannot continue
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Password cannot be changed without the old password", new[] { "oldPassword" }) });
            }

            //if an old password is supplied try to change it

            try
            {
                var result = membershipProvider.ChangePassword(username, passwordModel.OldPassword, passwordModel.NewPassword);

                return result == false
                    ? Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, invalid username or password", new[] { "oldPassword" }) })
                    : Attempt.Succeed(new PasswordChangedModel());
            }
            catch (Exception ex)
            {
                _logger.Warn<PasswordChanger>(ex, "Could not change member password");
                return Attempt.Fail(new PasswordChangedModel { ChangeError = new ValidationResult("Could not change password, error: " + ex.Message + " (see log for full details)", new[] { "value" }) });
            }

        }
    }
}
