// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification that is published after roles have been removed from external members.
/// </summary>
/// <remarks>
///     This notification is published by the <see cref="Services.IExternalMemberService"/> when RemoveRoles completes.
/// </remarks>
public class RemovedExternalMemberRolesNotification : ExternalMemberRolesNotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RemovedExternalMemberRolesNotification"/> class.
    /// </summary>
    /// <param name="memberKeys">The unique keys of the external members the roles are being removed from.</param>
    /// <param name="roles">The names of the roles being removed.</param>
    public RemovedExternalMemberRolesNotification(Guid[] memberKeys, string[] roles)
        : base(memberKeys, roles)
    {
    }
}
