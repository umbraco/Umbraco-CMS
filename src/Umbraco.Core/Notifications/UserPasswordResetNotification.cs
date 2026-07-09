// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user's password has been reset.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging and security monitoring purposes.
/// </remarks>
public class UserPasswordResetNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPasswordResetNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user resetting the password.</param>
    /// <param name="affectedUserId">The ID of the user whose password was reset.</param>
    /// <param name="performingUserId">The ID of the user performing the password reset.</param>
    public UserPasswordResetNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
