// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user account has been locked.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging and security monitoring purposes.
///     A user account may be locked after too many failed login attempts or by an administrator.
/// </remarks>
public class UserLockedNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserLockedNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address associated with the lock.</param>
    /// <param name="affectedUserId">The ID of the user whose account was locked.</param>
    /// <param name="performingUserId">The ID of the user or system that locked the account.</param>
    public UserLockedNotification(string ipAddress, string? affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
