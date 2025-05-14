using System.Globalization;
using Umbraco.Cms.Core;
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
    INotificationAsyncHandler<UserLoginSuccessNotification>,
    INotificationAsyncHandler<UserLogoutSuccessNotification>,
    INotificationAsyncHandler<UserLoginFailedNotification>,
    INotificationAsyncHandler<UserForgotPasswordRequestedNotification>,
    INotificationAsyncHandler<UserForgotPasswordChangedNotification>,
    INotificationAsyncHandler<UserPasswordChangedNotification>,
    INotificationAsyncHandler<UserPasswordResetNotification>
{
    private readonly IAuditEntryService _auditEntryService;
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeUserManagerAuditer"/> class.
    /// </summary>
    /// <param name="auditEntryService">The audit entry service.</param>
    /// <param name="userService">The user service.</param>
    public BackOfficeUserManagerAuditer(
        IAuditEntryService auditEntryService,
        IUserService userService)
    {
        _auditEntryService = auditEntryService;
        _userService = userService;
    }

    /// <inheritdoc />
    public Task HandleAsync(UserForgotPasswordChangedNotification notification, CancellationToken cancellationToken) =>
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/forgot/change",
            "password forgot/change");

    /// <inheritdoc />
    public Task HandleAsync(UserForgotPasswordRequestedNotification notification, CancellationToken cancellationToken) =>
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/forgot/request",
            "password forgot/request");

    /// <inheritdoc />
    public Task HandleAsync(UserLoginFailedNotification notification, CancellationToken cancellationToken) =>
        WriteAudit(
            notification.PerformingUserId,
            null,
            notification.IpAddress,
            "umbraco/user/sign-in/failed",
            "login failed");

    /// <inheritdoc />
    public Task HandleAsync(UserLoginSuccessNotification notification, CancellationToken cancellationToken)
        => WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/sign-in/login",
            "login success");

    /// <inheritdoc />
    public Task HandleAsync(UserLogoutSuccessNotification notification, CancellationToken cancellationToken)
        => WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/sign-in/logout",
            "logout success");

    /// <inheritdoc />
    public Task HandleAsync(UserPasswordChangedNotification notification, CancellationToken cancellationToken) =>
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/change",
            "password change");

    /// <inheritdoc />
    public Task HandleAsync(UserPasswordResetNotification notification, CancellationToken cancellationToken) =>
        WriteAudit(
            notification.PerformingUserId,
            notification.AffectedUserId,
            notification.IpAddress,
            "umbraco/user/password/reset",
            "password reset");

    private static string FormatEmail(IMembershipUser? user) =>
        user is null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? string.Empty : $"<{user.Email}>";

    private async Task WriteAudit(
        string performingId,
        string? affectedId,
        string ipAddress,
        string eventType,
        string eventDetails)
    {
        int? performingIdAsInt = ParseUserId(performingId);
        int? affectedIdAsInt = ParseUserId(affectedId);

        await WriteAudit(performingIdAsInt, affectedIdAsInt, ipAddress, eventType, eventDetails);
    }

    private static int? ParseUserId(string? id)
        => int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out var isAsInt) ? isAsInt : null;

    private async Task WriteAudit(
        int? performingId,
        int? affectedId,
        string ipAddress,
        string eventType,
        string eventDetails)
    {
        Guid? performingKey = null;
        var performingDetails = "User UNKNOWN:0";
        if (performingId.HasValue)
        {
            IUser? performingUser = _userService.GetUserById(performingId.Value);
            performingKey = performingUser?.Key;
            performingDetails = performingUser is null
                ? $"User UNKNOWN:{performingId.Value}"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";
        }

        Guid? affectedKey = null;
        var affectedDetails = "User UNKNOWN:0";
        if (affectedId.HasValue)
        {
            IUser? affectedUser = _userService.GetUserById(affectedId.Value);
            affectedKey = affectedUser?.Key;
            affectedDetails = affectedUser is null
                ? $"User UNKNOWN:{affectedId.Value}"
                : $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}";
        }

        await _auditEntryService.WriteAsync(
            performingKey ?? Constants.Security.UnknownUserKey,
            performingDetails,
            ipAddress,
            DateTime.UtcNow,
            affectedKey ?? Constants.Security.UnknownUserKey,
            affectedDetails,
            eventType,
            eventDetails);
    }
}
