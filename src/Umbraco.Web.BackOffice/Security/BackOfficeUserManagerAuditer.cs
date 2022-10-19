using System.Globalization;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Binds to notifications to write audit logs for the <see cref="BackOfficeUserManager" />
/// </summary>
internal class BackOfficeUserManagerAuditer :
    INotificationHandler<UserLoginSuccessNotification>,
    INotificationHandler<UserLogoutSuccessNotification>,
    INotificationHandler<UserLoginFailedNotification>,
    INotificationHandler<UserForgotPasswordRequestedNotification>,
    INotificationHandler<UserForgotPasswordChangedNotification>,
    INotificationHandler<UserPasswordChangedNotification>,
    INotificationHandler<UserPasswordResetNotification>
{
    private readonly IAuditService _auditService;
    private readonly IUserService _userService;

    public BackOfficeUserManagerAuditer(IAuditService auditService, IUserService userService)
    {
        _auditService = auditService;
        _userService = userService;
    }

    public void Handle(UserForgotPasswordChangedNotification notification) =>
        WriteAudit(notification.PerformingUserId, notification.AffectedUserId, notification.IpAddress,
            "umbraco/user/password/forgot/change", "password forgot/change");

    public void Handle(UserForgotPasswordRequestedNotification notification) =>
        WriteAudit(notification.PerformingUserId, notification.AffectedUserId, notification.IpAddress,
            "umbraco/user/password/forgot/request", "password forgot/request");

    public void Handle(UserLoginFailedNotification notification) =>
        WriteAudit(notification.PerformingUserId, "0", notification.IpAddress, "umbraco/user/sign-in/failed",
            "login failed", "");

    public void Handle(UserLoginSuccessNotification notification)
        => WriteAudit(notification.PerformingUserId, notification.AffectedUserId, notification.IpAddress,
            "umbraco/user/sign-in/login", "login success");

    public void Handle(UserLogoutSuccessNotification notification)
        => WriteAudit(notification.PerformingUserId, notification.AffectedUserId, notification.IpAddress,
            "umbraco/user/sign-in/logout", "logout success");

    public void Handle(UserPasswordChangedNotification notification) =>
        WriteAudit(notification.PerformingUserId, notification.AffectedUserId, notification.IpAddress,
            "umbraco/user/password/change", "password change");

    public void Handle(UserPasswordResetNotification notification) =>
        WriteAudit(notification.PerformingUserId, notification.AffectedUserId, notification.IpAddress,
            "umbraco/user/password/reset", "password reset");

    private static string FormatEmail(IMembershipUser user) =>
        user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? "" : $"<{user.Email}>";

    private void WriteAudit(string performingId, string? affectedId, string ipAddress, string eventType,
        string eventDetails, string? affectedDetails = null)
    {
        IUser? performingUser = null;
        if (int.TryParse(performingId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var asInt))
        {
            performingUser = _userService.GetUserById(asInt);
        }

        var performingDetails = performingUser == null
            ? $"User UNKNOWN:{performingId}"
            : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";

        if (!int.TryParse(performingId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var performingIdAsInt))
        {
            performingIdAsInt = 0;
        }

        if (!int.TryParse(affectedId, NumberStyles.Integer, CultureInfo.InvariantCulture, out var affectedIdAsInt))
        {
            affectedIdAsInt = 0;
        }

        WriteAudit(performingIdAsInt, performingDetails, affectedIdAsInt, ipAddress, eventType, eventDetails,
            affectedDetails);
    }

    private void WriteAudit(int performingId, string performingDetails, int affectedId, string ipAddress,
        string eventType, string eventDetails, string? affectedDetails = null)
    {
        if (affectedDetails == null)
        {
            IUser? affectedUser = _userService.GetUserById(affectedId);
            affectedDetails = affectedUser == null
                ? $"User UNKNOWN:{affectedId}"
                : $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}";
        }

        _auditService.Write(
            performingId,
            performingDetails,
            ipAddress,
            DateTime.UtcNow,
            affectedId,
            affectedDetails,
            eventType,
            eventDetails);
    }
}
