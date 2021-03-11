using System;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Runtime;
using Umbraco.Core.Services;

namespace Umbraco.Web.Install
{
    public class CreateUnattendUserComponent : IComponent
    {
        private IUserService _userService;

        public CreateUnattendUserComponent(IUserService userService)
        {
            _userService = userService;
        }

        public void Initialize()
        {
            // This event is fired when an Unattended Install has completed
            // Allowing us to auto generate the default super user
            CoreRuntime.UnattendedInstalled += CoreRuntime_UnattendedInstalled;
        }

        public void Terminate()
        {
            CoreRuntime.UnattendedInstalled -= CoreRuntime_UnattendedInstalled;
        }

        private void CoreRuntime_UnattendedInstalled(IRuntime sender, Core.Events.UnattendedInstallEventArgs e)
        {
            // TODO: Should AppSettings be read directly or need to be moved into main settings/config class ?
            var unattendedName = ConfigurationManager.AppSettings["Umbraco.Core.UnattendedUserName"];
            var unattendedEmail = ConfigurationManager.AppSettings["Umbraco.Core.UnattendedUserEmail"]; 
            var unattendedPassword = ConfigurationManager.AppSettings["Umbraco.Core.UnattendedUserPassword"];

            // Missing values
            if (unattendedName.IsNullOrWhiteSpace()
                || unattendedEmail.IsNullOrWhiteSpace()
                || unattendedPassword.IsNullOrWhiteSpace())
            {
                return;
            }

            var admin = _userService.GetUserById(Constants.Security.SuperUserId);
            if (admin == null)
            {
                throw new InvalidOperationException("Could not find the super user!");
            }

            // User email/login has already been modified
            if (admin.Email == unattendedEmail) return;

            // Everything looks good, create the user
            var membershipProvider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
            var superUser = membershipProvider.GetUser(Constants.Security.SuperUserId, true);
            if (superUser == null)
            {
                throw new InvalidOperationException($"No user found in membership provider with id of {Constants.Security.SuperUserId}.");
            }

            try
            {
                var success = superUser.ChangePassword("default", unattendedPassword);
                if (success == false)
                {
                    throw new FormatException("Password must be at least " + membershipProvider.MinRequiredPasswordLength + " characters long and contain at least " + membershipProvider.MinRequiredNonAlphanumericCharacters + " symbols");
                }
            }
            catch (Exception)
            {
                throw new FormatException("Password must be at least " + membershipProvider.MinRequiredPasswordLength + " characters long and contain at least " + membershipProvider.MinRequiredNonAlphanumericCharacters + " symbols");
            }

            // Set name, email & login
            admin.Name = unattendedName.Trim();
            admin.Email = unattendedEmail.Trim();
            admin.Username = unattendedEmail.Trim();

            _userService.Save(admin);
        }

    }
}
