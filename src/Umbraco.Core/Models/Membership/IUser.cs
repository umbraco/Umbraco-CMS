using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models.Membership;

/// <summary>
///     Defines the interface for a <see cref="User" />
/// </summary>
/// <remarks>Will be left internal until a proper Membership implementation is part of the roadmap</remarks>
public interface IUser : IMembershipUser, IRememberBeingDirty
{
    UserState UserState { get; }

    string? Name { get; set; }

    int SessionTimeout { get; set; }

    int[]? StartContentIds { get; set; }

    int[]? StartMediaIds { get; set; }

    string? Language { get; set; }

    DateTime? InvitedDate { get; set; }

    /// <summary>
    ///     Gets the groups that user is part of
    /// </summary>
    IEnumerable<IReadOnlyUserGroup> Groups { get; }

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
    ///     A Json blob stored for recording tour data for a user
    /// </summary>
    string? TourData { get; set; }

    void RemoveGroup(string group);

    void ClearGroups();

    void AddGroup(IReadOnlyUserGroup group);
}
