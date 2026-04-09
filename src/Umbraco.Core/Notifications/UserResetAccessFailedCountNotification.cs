// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when a user's failed access count has been reset.
/// </summary>
/// <remarks>
///     This notification is published when a user's account lockout counter is reset,
///     typically after a successful login or manual administrator action.
/// </remarks>
public class UserResetAccessFailedCountNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserResetAccessFailedCountNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the action.</param>
    /// <param name="affectedUserId">The ID of the user whose failed count was reset.</param>
    /// <param name="performingUserId">The ID of the user performing the reset.</param>
    public UserResetAccessFailedCountNotification(string ipAddress, string affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
