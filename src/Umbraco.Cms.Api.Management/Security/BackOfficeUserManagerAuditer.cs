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

    private async Task WriteAudit(
        string performingId,
        string? affectedId,
        string ipAddress,
        string eventType,
        string eventDetails)
    {
        var performingIdAsInt = ParseUserId(performingId);
        var affectedIdAsInt = ParseUserId(affectedId);

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
        IUser? performingUser = performingId is not null ? _userService.GetUserById(performingId.Value) : null;
        IUser? affectedUser = affectedId is not null ? _userService.GetUserById(affectedId.Value) : null;

        await _auditEntryService.WriteAsync(
            performingUser?.Key,
            FormatDetails(performingId, performingUser),
            ipAddress,
            DateTime.UtcNow,
            affectedUser?.Key,
            FormatDetails(affectedId, affectedUser),
            eventType,
            eventDetails);
    }

    private static string FormatDetails(int? id, IUser? user)
    {
        if (user == null)
        {
            return $"User UNKNOWN:{id ?? 0}";
        }

        return user.Email.IsNullOrWhiteSpace()
            ? $"User \"{user.Name}\""
            : $"User \"{user.Name}\" <{user.Email}>";
    }
}
