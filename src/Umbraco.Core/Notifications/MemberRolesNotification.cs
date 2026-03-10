// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications related to member role assignments.
/// </summary>
/// <remarks>
///     This class is used as a base for notifications published when member roles are assigned or removed.
/// </remarks>
public abstract class MemberRolesNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberRolesNotification"/> class.
    /// </summary>
    /// <param name="memberIds">The IDs of the members affected by the role change.</param>
    /// <param name="roles">The names of the roles being assigned or removed.</param>
    protected MemberRolesNotification(int[] memberIds, string[] roles)
    {
        MemberIds = memberIds;
        Roles = roles;
    }

    /// <summary>
    ///     Gets the IDs of the members affected by the role change.
    /// </summary>
    public int[] MemberIds { get; }

    /// <summary>
    ///     Gets the names of the roles being assigned or removed.
    /// </summary>
    public string[] Roles { get; }
}
