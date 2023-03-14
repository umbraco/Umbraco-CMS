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
    /// <param name="user"></param>
    /// <returns></returns>
    IPublishedContent? AsPublishedMember(MemberIdentityUser user);

    /// <summary>
    ///     Returns the currently logged in member if there is one, else returns null
    /// </summary>
    /// <returns></returns>
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
    /// <returns></returns>
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
    /// <param name="paths"></param>
    /// <returns></returns>
    Task<IReadOnlyDictionary<string, bool>> MemberHasAccessAsync(IEnumerable<string> paths);

    /// <summary>
    ///     Check if a document object is protected by the "Protect Pages" functionality in umbraco
    /// </summary>
    /// <param name="path">The full path of the document object to check</param>
    /// <returns>True if the document object is protected</returns>
    Task<bool> IsProtectedAsync(string path);

    Task<IReadOnlyDictionary<string, bool>> IsProtectedAsync(IEnumerable<string> paths);
}
