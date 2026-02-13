// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user's password has been changed.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging and security monitoring purposes.
/// </remarks>
public class UserPasswordChangedNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPasswordChangedNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user changing the password.</param>
    /// <param name="affectedUserId">The ID of the user whose password was changed.</param>
    /// <param name="performingUserId">The ID of the user performing the password change.</param>
    public UserPasswordChangedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(
        ipAddress, affectedUserId, performingUserId)
    {
    }
}
