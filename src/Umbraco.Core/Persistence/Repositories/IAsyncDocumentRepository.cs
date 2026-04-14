using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the async repository contract for <see cref="IContent" /> document entities.
/// </summary>
public interface IAsyncDocumentRepository : IAsyncPublishableContentRepository<IContent>
{
    /// <summary>
    ///     Gets a paged list of direct children of a document node, with template loading control.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="ordering">The ordering specification, or <c>null</c> for default ordering.</param>
    /// <param name="loadTemplates">
    ///     Whether to load templates. Set to <c>false</c> for performance optimization when templates are not
    ///     needed (e.g., collection views).
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching children and the total record count.</returns>
    new Task<PagedModel<IContent>> GetChildrenAsync(Guid parentKey, long pageIndex, int pageSize, string[]? propertyAliases, Ordering? ordering, bool loadTemplates, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of all descendants of a document node, with template loading control.
    /// </summary>
    /// <param name="ancestorKey">The Guid key of the ancestor node.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> for default ordering.</param>
    /// <param name="loadTemplates">Whether to load templates.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching descendants and the total record count.</returns>
    new Task<PagedModel<IContent>> GetDescendantsAsync(Guid ancestorKey, long pageIndex, int pageSize, Ordering? ordering, bool loadTemplates, CancellationToken cancellationToken);

    /// <summary>
    ///     Bulk-replaces all permissions for a content item with the provided permission set.
    /// </summary>
    /// <param name="permissionSet">The new set of permissions to apply, replacing any existing permissions.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ReplaceContentPermissionsAsync(EntityPermissionSet permissionSet, CancellationToken cancellationToken);

    /// <summary>
    ///     Assigns a single permission to the specified user groups for the given content item.
    /// </summary>
    /// <param name="entity">The content item to assign the permission to.</param>
    /// <param name="permission">The permission string to assign.</param>
    /// <param name="groupKeys">The Guid keys of the user groups receiving the permission.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AssignEntityPermissionAsync(IContent entity, string permission, IEnumerable<Guid> groupKeys, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the explicit list of permissions set on a content item.
    /// </summary>
    /// <param name="entityKey">The Guid key of the content item.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collection of permissions explicitly assigned to the content item.</returns>
    Task<EntityPermissionCollection> GetPermissionsForEntityAsync(Guid entityKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Adds or updates a permission for a content item.
    /// </summary>
    /// <param name="permission">The permission to add or update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddOrUpdatePermissionsAsync(ContentPermissionSet permission, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a value indicating whether the document recycle bin contains any content.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the recycle bin contains at least one document; otherwise, <c>false</c>.</returns>
    Task<bool> RecycleBinSmellsAsync(CancellationToken cancellationToken);
}
