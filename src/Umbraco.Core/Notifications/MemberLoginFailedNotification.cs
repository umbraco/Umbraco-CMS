// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published when a member login attempt fails.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging, security monitoring, and detecting potential
///     brute-force attacks. The <see cref="MemberNotification.MemberKey"/> may be <c>null</c> if the
///     member could not be found for the given credentials.
/// </remarks>
public class MemberLoginFailedNotification : MemberNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberLoginFailedNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the failed login attempt.</param>
    /// <param name="memberKey">The key of the member whose login failed, or <c>null</c> if the member could not be found.</param>
    public MemberLoginFailedNotification(string ipAddress, Guid? memberKey)
        : base(ipAddress, memberKey)
    {
    }
}
