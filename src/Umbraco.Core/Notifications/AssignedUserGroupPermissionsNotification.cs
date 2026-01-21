// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Notification published when user group permissions have been assigned to entities.
/// </summary>
/// <remarks>
///     This notification is published after permissions have been assigned, allowing handlers
///     to react to permission changes for auditing or cache invalidation purposes.
/// </remarks>
public class AssignedUserGroupPermissionsNotification : EnumerableObjectNotification<EntityPermission>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AssignedUserGroupPermissionsNotification"/> class.
    /// </summary>
    /// <param name="target">The entity permissions that were assigned.</param>
    /// <param name="messages">The event messages collection.</param>
    public AssignedUserGroupPermissionsNotification(IEnumerable<EntityPermission> target, EventMessages messages)
        : base(target, messages)
    {
    }

    /// <summary>
    ///     Gets the entity permissions that were assigned.
    /// </summary>
    public IEnumerable<EntityPermission> EntityPermissions => Target;
}
