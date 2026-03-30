using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The user manager for members
/// </summary>
public interface IMemberManager : IUmbracoUserManager<MemberIdentityUser>
{
    /// <summary>
    ///     Returns the <see cref="IPublishedContent" /> instance for the specified <see cref="MemberIdentityUser" />
    /// </summary>
    /// <param name="user">The member identity user.</param>
    /// <returns>The published content for the member, or null if not found.</returns>
    IPublishedContent? AsPublishedMember(MemberIdentityUser user);

    /// <summary>
    ///     Returns the currently logged in member if there is one, else returns null
    /// </summary>
    /// <returns>The current member, or null if not logged in.</returns>
    Task<MemberIdentityUser?> GetCurrentMemberAsync();

    /// <summary>
    ///     Checks if the current member is authorized based on the parameters provided.
    /// </summary>
    /// <param name="allowTypes">Allowed types.</param>
    /// <param name="allowGroups">Allowed groups.</param>
    /// <param name="allowMembers">Allowed individual members.</param>
    /// <returns>True or false if the currently logged in member is authorized</returns>
    Task<bool> IsMemberAuthorizedAsync(
        IEnumerable<string>? allowTypes = null,
        IEnumerable<string>? allowGroups = null,
        IEnumerable<int>? allowMembers = null);

    /// <summary>
    ///     Check if a member is logged in
    /// </summary>
    /// <returns>True if a member is logged in.</returns>
    bool IsLoggedIn();

    /// <summary>
    ///     Check if the current user has access to a document
    /// </summary>
    /// <param name="path">The full path of the document object to check</param>
    /// <returns>True if the current user has access or if the current document isn't protected</returns>
    Task<bool> MemberHasAccessAsync(string path);

    /// <summary>
    ///     Checks if the current user has access to the paths
    /// </summary>
    /// <param name="paths">The document paths to check access for.</param>
    /// <returns>A dictionary mapping each path to whether the member has access.</returns>
    Task<IReadOnlyDictionary<string, bool>> MemberHasAccessAsync(IEnumerable<string> paths);

    /// <summary>
    ///     Check if a document object is protected by the "Protect Pages" functionality in umbraco
    /// </summary>
    /// <param name="path">The full path of the document object to check</param>
    /// <returns>True if the document object is protected</returns>
    Task<bool> IsProtectedAsync(string path);

    /// <summary>
    /// Checks if the document objects specified by their paths are protected by the "Protect Pages" functionality in Umbraco.
    /// </summary>
    /// <param name="paths">The full paths of the document objects to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only dictionary mapping each document path to a boolean value indicating whether the corresponding document object is protected.</returns>
    Task<IReadOnlyDictionary<string, bool>> IsProtectedAsync(IEnumerable<string> paths);
}
