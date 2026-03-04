namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a model for adding or removing users from a user group.
/// </summary>
public class UsersToUserGroupManipulationModel
{
    /// <summary>
    ///     Gets the unique key of the user group to manipulate.
    /// </summary>
    public Guid UserGroupKey { get; init; }

    /// <summary>
    ///     Gets the array of user keys to add to or remove from the user group.
    /// </summary>
    public Guid[] UserKeys { get; init; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UsersToUserGroupManipulationModel" /> class.
    /// </summary>
    /// <param name="userGroupKey">The unique key of the user group.</param>
    /// <param name="userKeys">The array of user keys to manipulate.</param>
    public UsersToUserGroupManipulationModel(Guid userGroupKey, Guid[] userKeys)
    {
        UserGroupKey = userGroupKey;
        UserKeys = userKeys;
    }
}
