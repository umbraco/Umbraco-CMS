// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a member has successfully logged in.
/// </summary>
/// <remarks>
///     This notification is useful for audit logging, security monitoring, and post-authentication
///     concerns such as associating session data with a member.
/// </remarks>
public class MemberLoginSuccessNotification : MemberNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberLoginSuccessNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the member logging in.</param>
    /// <param name="memberKey">The key of the member who logged in.</param>
    public MemberLoginSuccessNotification(string ipAddress, Guid memberKey)
        : base(ipAddress, memberKey)
    {
    }
}
