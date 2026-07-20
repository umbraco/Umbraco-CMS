using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Defines the interface for a <see cref="User" />
/// </summary>
/// <remarks>Will be left internal until a proper Membership implementation is part of the roadmap</remarks>
public interface IUser : IMembershipUser, IRememberBeingDirty
{
    /// <summary>
    ///     Gets the current state of the user.
    /// </summary>
    UserState UserState { get; }

    /// <summary>
    ///     Gets or sets the display name of the user.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the session timeout in minutes.
    /// </summary>
    int SessionTimeout { get; set; }

    /// <summary>
    ///     Gets or sets the starting content node identifiers for this user.
    /// </summary>
    int[]? StartContentIds { get; set; }

    /// <summary>
    ///     Gets or sets the starting media node identifiers for this user.
    /// </summary>
    int[]? StartMediaIds { get; set; }

    /// <summary>
    ///     Gets or sets the preferred language for the user's backoffice UI.
    /// </summary>
    string? Language { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the user was invited.
    /// </summary>
    DateTime? InvitedDate { get; set; }

    /// <summary>
    ///     Gets the groups that user is part of
    /// </summary>
    IEnumerable<IReadOnlyUserGroup> Groups { get; }

    /// <summary>
    ///     Gets the collection of section aliases that this user has access to.
    /// </summary>
    IEnumerable<string> AllowedSections { get; }

    /// <summary>
    ///     Exposes the basic profile data
    /// </summary>
    IProfile ProfileData { get; }

    /// <summary>
    ///     Will hold the media file system relative path of the users custom avatar if they uploaded one
    /// </summary>
    string? Avatar { get; set; }

    /// <summary>
    ///     The type of user.
    /// </summary>
    UserKind Kind { get; set; }

    /// <summary>
    ///     Removes the user from the specified group.
    /// </summary>
    /// <param name="group">The alias of the group to remove the user from.</param>
    void RemoveGroup(string group);

    /// <summary>
    ///     Removes the user from all groups.
    /// </summary>
    void ClearGroups();

    /// <summary>
    ///     Adds the user to the specified group.
    /// </summary>
    /// <param name="group">The group to add the user to.</param>
    void AddGroup(IReadOnlyUserGroup group);
}
