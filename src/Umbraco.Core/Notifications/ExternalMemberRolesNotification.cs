// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Abstract base class for notifications related to external member role assignments.
/// </summary>
/// <remarks>
///     This class is used as a base for notifications published when external member roles are assigned or removed.
/// </remarks>
public abstract class ExternalMemberRolesNotification : INotification
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberRolesNotification"/> class.
    /// </summary>
    /// <param name="memberKeys">The unique keys of the external members affected by the role change.</param>
    /// <param name="roles">The names of the roles being assigned or removed.</param>
    protected ExternalMemberRolesNotification(Guid[] memberKeys, string[] roles)
    {
        MemberKeys = memberKeys;
        Roles = roles;
    }

    /// <summary>
    ///     Gets the unique keys of the external members affected by the role change.
    /// </summary>
    public Guid[] MemberKeys { get; }

    /// <summary>
    ///     Gets the names of the roles being assigned or removed.
    /// </summary>
    public string[] Roles { get; }
}
