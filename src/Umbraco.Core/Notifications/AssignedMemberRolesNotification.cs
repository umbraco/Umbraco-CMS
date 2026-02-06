// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after roles have been assigned to members.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IMemberService"/> when AssignRoles or ReplaceRoles methods complete.
/// </remarks>
public class AssignedMemberRolesNotification : MemberRolesNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AssignedMemberRolesNotification"/> class.
    /// </summary>
    /// <param name="memberIds">The IDs of the members the roles are being assigned to.</param>
    /// <param name="roles">The names of the roles being assigned.</param>
    public AssignedMemberRolesNotification(int[] memberIds, string[] roles)
        : base(memberIds, roles)
    {
    }
}
