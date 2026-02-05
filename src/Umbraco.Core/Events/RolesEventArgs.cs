namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents event data for role assignment operations.
/// </summary>
public class RolesEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RolesEventArgs" /> class.
    /// </summary>
    /// <param name="memberIds">The identifiers of the members being assigned roles.</param>
    /// <param name="roles">The names of the roles being assigned.</param>
    public RolesEventArgs(int[] memberIds, string[] roles)
    {
        MemberIds = memberIds;
        Roles = roles;
    }

    /// <summary>
    ///     Gets or sets the identifiers of the members being assigned roles.
    /// </summary>
    public int[] MemberIds { get; set; }

    /// <summary>
    ///     Gets or sets the names of the roles being assigned.
    /// </summary>
    public string[] Roles { get; set; }
}
