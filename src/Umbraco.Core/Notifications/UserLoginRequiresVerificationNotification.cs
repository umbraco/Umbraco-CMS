// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published when a user login requires additional verification (e.g., two-factor authentication).
/// </summary>
/// <remarks>
///     This notification is published when a user successfully provides their credentials but
///     needs to complete an additional verification step before being fully logged in.
/// </remarks>
public class UserLoginRequiresVerificationNotification : UserNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserLoginRequiresVerificationNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the user.</param>
    /// <param name="affectedUserId">The ID of the user requiring verification.</param>
    /// <param name="performingUserId">The ID of the user performing the login action.</param>
    public UserLoginRequiresVerificationNotification(string ipAddress, string? affectedUserId, string performingUserId)
        : base(ipAddress, affectedUserId, performingUserId)
    {
    }
}
