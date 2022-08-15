using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Install;

public class CreateUnattendedUserNotificationHandler : INotificationAsyncHandler<UnattendedInstallNotification>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<UnattendedSettings> _unattendedSettings;
    private readonly IUserService _userService;

    public CreateUnattendedUserNotificationHandler(IOptions<UnattendedSettings> unattendedSettings,
        IUserService userService, IServiceScopeFactory serviceScopeFactory)
    {
        _unattendedSettings = unattendedSettings;
        _userService = userService;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    ///     Listening for when the UnattendedInstallNotification fired after a sucessfulk
    /// </summary>
    /// <param name="notification"></param>
    public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
    {
        UnattendedSettings? unattendedSettings = _unattendedSettings.Value;
        // Ensure we have the setting enabled (Sanity check)
        // In theory this should always be true as the event only fired when a sucessfull
        if (_unattendedSettings.Value.InstallUnattended == false)
        {
            return;
        }

        var unattendedName = unattendedSettings.UnattendedUserName;
        var unattendedEmail = unattendedSettings.UnattendedUserEmail;
        var unattendedPassword = unattendedSettings.UnattendedUserPassword;

        // Missing configuration values (json, env variables etc)
        if (unattendedName.IsNullOrWhiteSpace()
            || unattendedEmail.IsNullOrWhiteSpace()
            || unattendedPassword.IsNullOrWhiteSpace())
        {
            return;
        }

        IUser? admin = _userService.GetUserById(Constants.Security.SuperUserId);
        if (admin == null)
        {
            throw new InvalidOperationException("Could not find the super user!");
        }

        // User email/login has already been modified
        if (admin.Email == unattendedEmail)
        {
            return;
        }

        // Update name, email & login & save user
        admin.Name = unattendedName!.Trim();
        admin.Email = unattendedEmail!.Trim();
        admin.Username = unattendedEmail.Trim();
        _userService.Save(admin);

        // Change Password for the default user we ship out of the box
        // Uses same approach as NewInstall Step
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IBackOfficeUserManager backOfficeUserManager =
            scope.ServiceProvider.GetRequiredService<IBackOfficeUserManager>();
        BackOfficeIdentityUser membershipUser =
            await backOfficeUserManager.FindByIdAsync(Constants.Security.SuperUserIdAsString);
        if (membershipUser == null)
        {
            throw new InvalidOperationException(
                $"No user found in membership provider with id of {Constants.Security.SuperUserIdAsString}.");
        }

        //To change the password here we actually need to reset it since we don't have an old one to use to change
        var resetToken = await backOfficeUserManager.GeneratePasswordResetTokenAsync(membershipUser);
        if (string.IsNullOrWhiteSpace(resetToken))
        {
            throw new InvalidOperationException("Could not reset password: unable to generate internal reset token");
        }

        IdentityResult resetResult =
            await backOfficeUserManager.ChangePasswordWithResetAsync(membershipUser.Id, resetToken,
                unattendedPassword!.Trim());
        if (!resetResult.Succeeded)
        {
            throw new InvalidOperationException("Could not reset password: " +
                                                string.Join(", ", resetResult.Errors.ToErrorMessage()));
        }
    }
}
