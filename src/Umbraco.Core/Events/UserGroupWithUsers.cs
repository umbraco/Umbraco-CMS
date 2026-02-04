using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Represents a user group along with the users that were added to or removed from it.
/// </summary>
public class UserGroupWithUsers
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupWithUsers" /> class.
    /// </summary>
    /// <param name="userGroup">The user group.</param>
    /// <param name="addedUsers">The users that were added to the group.</param>
    /// <param name="removedUsers">The users that were removed from the group.</param>
    public UserGroupWithUsers(IUserGroup userGroup, IUser[] addedUsers, IUser[] removedUsers)
    {
        UserGroup = userGroup;
        AddedUsers = addedUsers;
        RemovedUsers = removedUsers;
    }

    /// <summary>
    ///     Gets the user group.
    /// </summary>
    public IUserGroup UserGroup { get; }

    /// <summary>
    ///     Gets the users that were added to the group.
    /// </summary>
    public IUser[] AddedUsers { get; }

    /// <summary>
    ///     Gets the users that were removed from the group.
    /// </summary>
    public IUser[] RemovedUsers { get; }
}
