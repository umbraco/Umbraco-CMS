// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a user account has been unlocked.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging and security monitoring purposes.
/// </remarks>
public class UserUnlockedNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserUnlockedNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address associated with the unlock.</param>
    /// <param name="affectedUserId">The ID of the user whose account was unlocked.</param>
    /// <param name="performingUserId">The ID of the user performing the unlock.</param>
    public UserUnlockedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
