using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a resolved model for manipulating user-to-user-group assignments.
/// </summary>
public class ResolvedUserToUserGroupManipulationModel
{
    /// <summary>
    /// Gets the array of users involved in the manipulation operation.
    /// </summary>
    public required IUser[] Users { get; init; }

    /// <summary>
    /// Gets the user group that the users are being added to or removed from.
    /// </summary>
    public required IUserGroup UserGroup { get; init; }
}
