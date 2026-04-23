// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after roles have been assigned to external members.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IExternalMemberService"/> when AssignRoles completes.
/// </remarks>
public class AssignedExternalMemberRolesNotification : ExternalMemberRolesNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AssignedExternalMemberRolesNotification"/> class.
    /// </summary>
    /// <param name="memberKeys">The unique keys of the external members the roles are being assigned to.</param>
    /// <param name="roles">The names of the roles being assigned.</param>
    public AssignedExternalMemberRolesNotification(Guid[] memberKeys, string[] roles)
        : base(memberKeys, roles)
    {
    }
}
