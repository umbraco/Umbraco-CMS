using System.Security.Claims;
using Examine;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Authorizes access to Examine indexes that hold member data.
/// </summary>
/// <remarks>
/// Member indexes hold login names, email addresses and member property values, so access to them
/// requires access to the members section - not only to the settings section that the Examine
/// management endpoints require.
/// </remarks>
public interface IMemberIndexAuthorizer
{
    /// <summary>
    /// Gets a value indicating whether the supplied index holds member data.
    /// </summary>
    /// <param name="index">The index to check.</param>
    /// <returns><c>true</c> if the index holds member data; otherwise, <c>false</c>.</returns>
    bool IsMemberIndex(IIndex index);

    /// <summary>
    /// Gets a value indicating whether the supplied name identifies an index holding member data.
    /// </summary>
    /// <param name="indexOrSearcherName">The name of the index, or of the index's searcher, to check.</param>
    /// <returns><c>true</c> if a matching index exists and holds member data; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Both names are matched because a searcher can be resolved either by its index name or by its own
    /// name - see <see cref="IExamineManagerService.TryFindSearcher" />. A searcher registered on its own,
    /// without a backing index, cannot be classified and is therefore treated as not holding member data.
    /// </remarks>
    bool IsMemberIndex(string indexOrSearcherName);

    /// <summary>
    /// Gets a value indicating whether the supplied principal may access member data.
    /// </summary>
    /// <param name="principal">The principal to authorize.</param>
    /// <returns><c>true</c> if the principal may access member data; otherwise, <c>false</c>.</returns>
    Task<bool> HasAccessAsync(ClaimsPrincipal principal);
}
