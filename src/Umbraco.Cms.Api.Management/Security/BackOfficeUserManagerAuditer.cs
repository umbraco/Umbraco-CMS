using System.Globalization;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Binds to notifications to write audit logs for the <see cref="BackOfficeUserManager" />
/// </summary>
internal sealed class BackOfficeUserManagerAuditer :
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
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/forgot/change",
            "password forgot/change");

    public void Handle(UserForgotPasswordRequestedNotification notification) =>
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/forgot/request",
            "password forgot/request");

    public void Handle(UserLoginFailedNotification notification) =>
        WriteAudit(
            notification.PerformingUserId,
            null,
            notification.IpAddress,
            "umbraco/user/sign-in/failed",
            "login failed");

    public void Handle(UserLoginSuccessNotification notification)
        => WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/sign-in/login",
            "login success");

    public void Handle(UserLogoutSuccessNotification notification)
        => WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/sign-in/logout",
            "logout success");

    public void Handle(UserPasswordChangedNotification notification) =>
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/change",
            "password change");

    public void Handle(UserPasswordResetNotification notification) =>
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/reset",
            "password reset");

    private static string FormatEmail(IMembershipUser? user) =>
        user is null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? string.Empty : $"<{user.Email}>";

    private void WriteAudit(
        string performingId,
        string? affectedId,
        string ipAddress,
        string eventType,
        string eventDetails)
    {
        int? performingIdAsInt = ParseUserId(performingId);
        int? affectedIdAsInt = ParseUserId(affectedId);

        WriteAudit(performingIdAsInt, affectedIdAsInt, ipAddress, eventType, eventDetails);
    }

    private static int? ParseUserId(string? id)
        => int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var isAsInt) ? isAsInt : null;

    private void WriteAudit(
        int? performingId,
        int? affectedId,
        string ipAddress,
        string eventType,
        string eventDetails)
    {
        var performingDetails = "User UNKNOWN:0";
        if (performingId.HasValue)
        {
            IUser? performingUser = _userService.GetUserById(performingId.Value);
            performingDetails = performingUser is null
                ? $"User UNKNOWN:{performingId.Value}"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";
        }

        var affectedDetails = "User UNKNOWN:0";
        if (affectedId.HasValue)
        {
            IUser? affectedUser = _userService.GetUserById(affectedId.Value);
            affectedDetails = affectedUser is null
                ? $"User UNKNOWN:{affectedId.Value}"
                : $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}";
        }

        _auditService.Write(
            performingId ?? 0,
            performingDetails,
            ipAddress,
            DateTime.UtcNow,
            affectedId ?? 0,
            affectedDetails,
            eventType,
            eventDetails);
    }
}
