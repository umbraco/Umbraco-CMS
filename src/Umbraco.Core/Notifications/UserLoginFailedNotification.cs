// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published when a user login attempt fails.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging, security monitoring, and detecting potential brute-force attacks.
/// </remarks>
public class UserLoginFailedNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserLoginFailedNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the failed login attempt.</param>
    /// <param name="affectedUserId">The ID of the user whose login failed.</param>
    /// <param name="performingUserId">The ID of the user attempting the login.</param>
    public UserLoginFailedNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(
        ipAddress, affectedUserId, performingUserId)
    {
    }
}
