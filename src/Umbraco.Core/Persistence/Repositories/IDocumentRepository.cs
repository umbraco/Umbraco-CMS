using System.Collections.Immutable;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IContent" /> document entities.
/// </summary>
public interface IDocumentRepository : IContentRepository<int, IContent>, IReadRepository<Guid, IContent>
{
    /// <summary>
    ///     Gets paged documents.
    /// </summary>
    /// <param name="query">The base query for documents.</param>
    /// <param name="pageIndex">The page index (zero-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalRecords">Output parameter with total record count.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If null, all properties are loaded.
    ///     If empty array, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="filter">Optional filter query.</param>
    /// <param name="ordering">The ordering specification.</param>
    /// <param name="loadTemplates">
    ///     Whether to load templates. Set to false for performance optimization when templates are not needed
    ///     (e.g., collection views). Default is true.
    /// </param>
    /// <returns>A collection of documents for the specified page.</returns>
    /// <remarks>Here, <paramref name="filter" /> can be null but <paramref name="ordering" /> cannot.</remarks>
    IEnumerable<IContent> GetPage(
        IQuery<IContent>? query,
        long pageIndex,
        int pageSize,
        out long totalRecords,
        string[]? propertyAliases,
        IQuery<IContent>? filter,
        Ordering? ordering,
        bool loadTemplates)
        => GetPage(query, pageIndex, pageSize, out totalRecords, propertyAliases, filter, ordering);

    /// <summary>
    ///     Gets publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns>
    ///     <see cref="ContentScheduleCollection" />
    /// </returns>
    ContentScheduleCollection GetContentSchedule(int contentId);

    /// <summary>
    ///     Persists publish/unpublish schedule for a content node.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="schedule"></param>
    void PersistContentSchedule(IContent content, ContentScheduleCollection schedule);

    /// <summary>
    ///     Clears the publishing schedule for all entries having an a date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <summary>
    ///     Clears the publishing schedule for all entries having a date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <param name="date">The cutoff date.</param>
    void ClearSchedule(DateTime date);

    /// <summary>
    ///     Clears the publishing schedule for entries matching the specified action and having a date before the specified date.
    /// </summary>
    /// <param name="date">The cutoff date.</param>
    /// <param name="action">The schedule action to clear.</param>
    void ClearSchedule(DateTime date, ContentScheduleAction action);

    /// <summary>
    ///     Checks whether there is content scheduled for expiration before the specified date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns><c>true</c> if there is content scheduled for expiration; otherwise, <c>false</c>.</returns>
    bool HasContentForExpiration(DateTime date);

    /// <summary>
    ///     Checks whether there is content scheduled for release before the specified date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns><c>true</c> if there is content scheduled for release; otherwise, <c>false</c>.</returns>
    bool HasContentForRelease(DateTime date);

    /// <summary>
    ///     Gets <see cref="IContent" /> objects having an expiration date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<IContent> GetContentForExpiration(DateTime date);

    /// <summary>
    ///     Gets <see cref="IContent" /> objects having a release date before (lower than, or equal to) a specified date.
    /// </summary>
    /// <remarks>
    ///     The content returned from this method may be culture variant, in which case you can use
    ///     <see cref="Umbraco.Extensions.ContentExtensions.GetStatus(IContent, ContentScheduleCollection, string?)" /> to get the status for a specific culture.
    /// </remarks>
    IEnumerable<IContent> GetContentForRelease(DateTime date);

    /// <summary>
    ///     Gets the content keys from the provided collection of keys that are scheduled for publishing.
    /// </summary>
    /// <param name="documentIds">The IDs of the documents.</param>
    /// <returns>
    ///     The provided collection of content keys filtered for those that are scheduled for publishing.
    /// </returns>
    IDictionary<int, IEnumerable<ContentSchedule>> GetContentSchedulesByIds(int[] documentIds) => ImmutableDictionary<int, IEnumerable<ContentSchedule>>.Empty;

    /// <summary>
    ///     Get the count of published items
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     We require this on the repo because the IQuery{IContent} cannot supply the 'newest' parameter
    /// </remarks>
    int CountPublished(string? contentTypeAlias = null);

    /// <summary>
    ///     Checks whether the path to a content item is published.
    /// </summary>
    /// <param name="content">The content item.</param>
    /// <returns><c>true</c> if the path is published; otherwise, <c>false</c>.</returns>
    bool IsPathPublished(IContent? content);

    /// <summary>
    ///     Used to bulk update the permissions set for a content item. This will replace all permissions
    ///     assigned to an entity with a list of user id &amp; permission pairs.
    /// </summary>
    /// <param name="permissionSet"></param>
    void ReplaceContentPermissions(EntityPermissionSet permissionSet);

    /// <summary>
    ///     Assigns a single permission to the current content item for the specified user group ids
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="permission"></param>
    /// <param name="groupIds"></param>
    void AssignEntityPermission(IContent entity, string permission, IEnumerable<int> groupIds);

    /// <summary>
    ///     Gets the explicit list of permissions for the content item
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    EntityPermissionCollection GetPermissionsForEntity(int entityId);

    /// <summary>
    ///     Used to add/update a permission for a content item
    /// </summary>
    /// <param name="permission"></param>
    void AddOrUpdatePermissions(ContentPermissionSet permission);

    /// <summary>
    ///     Returns true if there is any content in the recycle bin
    /// </summary>
    bool RecycleBinSmells();
}
