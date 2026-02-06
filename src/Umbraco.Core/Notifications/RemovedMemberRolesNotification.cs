// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after roles have been removed from members.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMemberService"/> when DissociateRoles method completes.
/// </remarks>
public class RemovedMemberRolesNotification : MemberRolesNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RemovedMemberRolesNotification"/> class.
    /// </summary>
    /// <param name="memberIds">The IDs of the members the roles are being removed from.</param>
    /// <param name="roles">The names of the roles being removed.</param>
    public RemovedMemberRolesNotification(int[] memberIds, string[] roles)
        : base(memberIds, roles)
    {
    }
}
