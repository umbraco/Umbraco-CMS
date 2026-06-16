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
    ///     Gets a paged list of direct children of a document node, without loading template information.
    /// </summary>
    /// <remarks>
    ///     Use this overload when template IDs are not required (e.g. collection/list views) to avoid the
    ///     template-existence validation round-trip against the template repository.
    /// </remarks>
    /// <param name="parentKey">The Guid key of the parent node.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="ordering">The ordering specification, or <c>null</c> for default ordering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching children with <c>null</c> template IDs.</returns>
    Task<PagedModel<IContent>> GetChildrenWithoutTemplatesAsync(Guid parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of all descendants of a document node, without loading template information.
    /// </summary>
    /// <remarks>
    ///     Use this overload when template IDs are not required (e.g. collection/list views) to avoid the
    ///     template-existence validation round-trip against the template repository.
    /// </remarks>
    /// <param name="ancestorKey">The Guid key of the ancestor node.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> for default ordering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching descendants with <c>null</c> template IDs.</returns>
    Task<PagedModel<IContent>> GetDescendantsWithoutTemplatesAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

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
