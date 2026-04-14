// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after a member has successfully logged out.
/// </summary>
public class MemberLogoutSuccessNotification : MemberNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberLogoutSuccessNotification"/> class.
    /// </summary>
    /// <param name="ipAddress">The source IP address of the member logging out.</param>
    /// <param name="memberKey">The key of the member who logged out.</param>
    public MemberLogoutSuccessNotification(string ipAddress, Guid memberKey)
        : base(ipAddress, memberKey)
    {
    }
}
