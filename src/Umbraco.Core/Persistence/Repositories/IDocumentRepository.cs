using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDocumentRepository : IPublishableContentRepository<IContent>
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
        bool loadTemplates);

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
