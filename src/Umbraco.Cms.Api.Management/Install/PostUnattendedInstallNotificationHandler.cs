using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Install;

/// <summary>
/// Handles notifications that are triggered after the completion of an unattended installation process.
/// </summary>
public class PostUnattendedInstallNotificationHandler : INotificationAsyncHandler<UnattendedInstallNotification>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<UnattendedSettings> _unattendedSettings;
    private readonly IUserService _userService;
    private readonly IMetricsConsentService _metricsConsentService;
    private readonly IHmacSecretKeyService _hmacSecretKeyService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PostUnattendedInstallNotificationHandler" /> class.
    /// </summary>
    /// <param name="unattendedSettings">The unattended settings.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="metricsConsentService">The metrics consent service.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public PostUnattendedInstallNotificationHandler(
        IOptions<UnattendedSettings> unattendedSettings,
        IUserService userService,
        IServiceScopeFactory serviceScopeFactory,
        IMetricsConsentService metricsConsentService)
        : this(
            unattendedSettings,
            userService,
            serviceScopeFactory,
            metricsConsentService,
            StaticServiceProvider.Instance.GetRequiredService<IHmacSecretKeyService>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PostUnattendedInstallNotificationHandler" /> class.
    /// </summary>
    /// <param name="unattendedSettings">The unattended settings.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    /// <param name="metricsConsentService">The metrics consent service.</param>
    /// <param name="hmacSecretKeyService">The HMAC secret key service.</param>
    public PostUnattendedInstallNotificationHandler(
        IOptions<UnattendedSettings> unattendedSettings,
        IUserService userService,
        IServiceScopeFactory serviceScopeFactory,
        IMetricsConsentService metricsConsentService,
        IHmacSecretKeyService hmacSecretKeyService)
    {
        _unattendedSettings = unattendedSettings;
        _userService = userService;
        _serviceScopeFactory = serviceScopeFactory;
        _metricsConsentService = metricsConsentService;
        _hmacSecretKeyService = hmacSecretKeyService;
    }

    /// <summary>
    /// Handles the <see cref="UnattendedInstallNotification"/> event, which is fired after a successful unattended install.
    /// This method creates the user and sets the telemetry level based on the 'Unattended' settings.
    /// </summary>
    /// <param name="notification">The notification containing information about the unattended install.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task HandleAsync(UnattendedInstallNotification notification, CancellationToken cancellationToken)
    {
        UnattendedSettings? unattendedSettings = _unattendedSettings.Value;

        // Ensure we have the setting enabled (Sanity check)
        // In theory this should always be true as the event only fired when a sucessfull
        if (_unattendedSettings.Value.InstallUnattended == false)
        {
            return;
        }

        // Generate an imaging HMAC secret key if one is not already configured.
        await _hmacSecretKeyService.CreateHmacSecretKeyAsync();

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

        IUser? admin = await _userService.GetAsync(Constants.Security.SuperUserKey);
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
        BackOfficeIdentityUser? membershipUser =
            await backOfficeUserManager.FindByIdAsync(Constants.Security.SuperUserIdAsString);
        if (membershipUser == null)
        {
            throw new InvalidOperationException(
                $"No user found in membership provider with id of {Constants.Security.SuperUserIdAsString}.");
        }

        // To change the password here we actually need to reset it since we don't have an old one to use to change
        var resetToken = await backOfficeUserManager.GeneratePasswordResetTokenAsync(membershipUser);
        if (string.IsNullOrWhiteSpace(resetToken))
        {
            throw new InvalidOperationException("Could not reset password: unable to generate internal reset token");
        }

        IdentityResult resetResult =
            await backOfficeUserManager.ChangePasswordWithResetAsync(
                membershipUser.Id,
                resetToken,
                unattendedPassword!.Trim());
        if (!resetResult.Succeeded)
        {
            throw new InvalidOperationException("Could not reset password: " +
                                                string.Join(", ", resetResult.Errors.ToErrorMessage()));
        }

        await _metricsConsentService.SetConsentLevelAsync(unattendedSettings.UnattendedTelemetryLevel);
    }
}
