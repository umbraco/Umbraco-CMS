using System;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Events;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Install
{
    public class CreateUnattendedUserNotificationHandler : INotificationHandler<UmbracoApplicationStarting>
    {
        private readonly GlobalSettings _globalSettings;
        private readonly IUserService _userService;
        private readonly IBackOfficeUserManager _userManager;

        public CreateUnattendedUserNotificationHandler(IOptions<GlobalSettings> globalSettings, IUserService userService, IBackOfficeUserManager userManager)
        {
            _globalSettings = globalSettings.Value;
            _userService = userService;
            _userManager = userManager;
        }

        /// Listening for when the UnattendedInstallNotification fired after a sucessfulk
        /// </summary>
        /// <param name="notification"></param>
        public async void Handle(UmbracoApplicationStarting notification)
        {
            // Ensure we have the setting enabled (Sanity check)
            // In theory this should always be true as the event only fired when a sucessfull
            if (_globalSettings.InstallUnattended == false)
                return;

            var unattendedName = _globalSettings.UnattendedUserName;
            var unattendedEmail = _globalSettings.UnattendedUserEmail;
            var unattendedPassword = _globalSettings.UnattendedUserPassword;

            // Missing configuration values (json, env variables etc)
            if (unattendedName.IsNullOrWhiteSpace()
                || unattendedEmail.IsNullOrWhiteSpace()
                || unattendedPassword.IsNullOrWhiteSpace())
            {
                return;
            }

            var admin = _userService.GetUserById(Core.Constants.Security.SuperUserId);
            if (admin == null)
            {
                throw new InvalidOperationException("Could not find the super user!");
            }

            // User email/login has already been modified
            if (admin.Email == unattendedEmail)
                return;

            // Update name, email & login & save user
            admin.Name = unattendedName.Trim();
            admin.Email = unattendedEmail.Trim();
            admin.Username = unattendedEmail.Trim();
            _userService.Save(admin);

            // Change Password for the default user we ship out of the box
            // Uses same approach as NewInstall Step
            var membershipUser = await _userManager.FindByIdAsync(Core.Constants.Security.SuperUserId.ToString());
            if (membershipUser == null)
            {
                throw new InvalidOperationException($"No user found in membership provider with id of {Core.Constants.Security.SuperUserId}.");
            }

            //To change the password here we actually need to reset it since we don't have an old one to use to change
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(membershipUser);
            if (string.IsNullOrWhiteSpace(resetToken))
                throw new InvalidOperationException("Could not reset password: unable to generate internal reset token");

            var resetResult = await _userManager.ChangePasswordWithResetAsync(membershipUser.Id, resetToken, unattendedPassword.Trim());
            if (!resetResult.Succeeded)
                throw new InvalidOperationException("Could not reset password: " + string.Join(", ", resetResult.Errors.ToErrorMessage()));

            throw new NotImplementedException();
        }

    }
}
