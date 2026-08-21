using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Defines the base async repository contract for content items.
/// </summary>
public interface IAsyncContentRepository<TEntity> : IAsyncReadWriteRepository<Guid, TEntity>
    where TEntity : IUmbracoEntity
{
    /// <summary>
    ///     Gets the Guid key of the recycle bin node for this content type.
    /// </summary>
    Guid RecycleBinKey { get; }

    /// <summary>
    ///     Updates the sort order of the specified nodes so that each node's sort order matches its
    ///     position in the supplied (already ordered) collection, in a single batched operation.
    /// </summary>
    /// <param name="orderedNodeKeys">The Guid keys of the nodes, in their desired order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>
    ///     This persists the sort order directly and does not load the entities or fire any notifications;
    ///     callers are responsible for any required cache refresh and auditing.
    /// </remarks>
    Task UpdateSortOrderAsync(IReadOnlyList<Guid> orderedNodeKeys, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all versions of a content node.
    /// </summary>
    /// <param name="nodeKey">The Guid key of the content node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     All versions of the node, with the current version first and subsequent versions ordered most recent first.
    /// </returns>
    Task<IEnumerable<TEntity>> GetAllVersionsAsync(Guid nodeKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a slim (reduced data) page of versions for a content node.
    /// </summary>
    /// <param name="nodeKey">The Guid key of the content node.</param>
    /// <param name="skip">The number of versions to skip.</param>
    /// <param name="take">The maximum number of versions to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A subset of versions for the node, with the current version first and subsequent versions ordered most recent first.
    /// </returns>
    Task<IEnumerable<TEntity>> GetAllVersionsSlimAsync(Guid nodeKey, int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves the Guid keys of all versions for the specified node, with the current version first, followed by
    ///     previous versions in descending order by version date.
    /// </summary>
    /// <param name="nodeKey">The Guid key of the content node whose version keys are to be retrieved.</param>
    /// <param name="maxRows">The maximum number of version keys to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     Up to <paramref name="maxRows" /> version keys, ordered with the current version first and older versions
    ///     following in descending order by date.
    /// </returns>
    Task<IEnumerable<Guid>> GetVersionKeysAsync(Guid nodeKey, int maxRows, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a specific version of a content node by its Guid key.
    /// </summary>
    /// <param name="versionKey">The Guid key of the version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content entity at the specified version, or <c>null</c> if not found.</returns>
    Task<TEntity?> GetVersionAsync(Guid versionKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a specific version by its Guid key.
    /// </summary>
    /// <param name="versionKey">The Guid key of the version to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteVersionAsync(Guid versionKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes all versions of a content node that are older than the specified date.
    /// </summary>
    /// <param name="nodeKey">The Guid key of the content node.</param>
    /// <param name="versionDate">Versions with a date strictly before this value will be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteVersionsAsync(Guid nodeKey, DateTime versionDate, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the total count of content items.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total number of content items.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the total count of content items of the specified content type.
    /// </summary>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of content items of the specified type.</returns>
    Task<int> CountAsync(string contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the count of direct child content items under a given parent.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of direct children.</returns>
    Task<int> CountChildrenAsync(Guid parentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the count of direct child content items under a given parent, filtered by content type alias.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of direct children of the specified type.</returns>
    Task<int> CountChildrenAsync(Guid parentKey, string contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the count of all descendant content items under a given ancestor.
    /// </summary>
    /// <param name="parentKey">The Guid key of the ancestor node.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of descendants.</returns>
    Task<int> CountDescendantsAsync(Guid parentKey, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the count of all descendant content items under a given ancestor, filtered by content type alias.
    /// </summary>
    /// <param name="parentKey">The Guid key of the ancestor node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of descendants of the specified type.</returns>
    Task<int> CountDescendantsAsync(Guid parentKey, string contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of direct children of a content node.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node, or <c>null</c> for the root of the content tree.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="ordering">The ordering specification. Must not be <c>null</c> — callers that don't have an opinion should use a service-layer facade that applies a default.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching children and the total record count.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ordering" /> is <c>null</c>.</exception>
    Task<PagedModel<TEntity>> GetChildrenAsync(Guid? parentKey, int skip, int take, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of all descendants of a content node.
    /// </summary>
    /// <param name="ancestorKey">The Guid key of the ancestor node.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification. Must not be <c>null</c> — callers that don't have an opinion should use a service-layer facade that applies a default.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="includeTrashed">Whether to include descendants that are currently in the recycle bin. Default is <c>true</c>.</param>
    /// <returns>A paged result containing the matching descendants and the total record count.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ordering" /> is <c>null</c>.</exception>
    Task<PagedModel<TEntity>> GetDescendantsAsync(Guid ancestorKey, int skip, int take, Ordering? ordering, CancellationToken cancellationToken, bool includeTrashed = true);

    /// <summary>
    ///     Gets the content items that reside at the root of the content tree.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content items at the root of the tree.</returns>
    Task<IEnumerable<TEntity>> GetRootContentAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all content items currently in the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All content items in the recycle bin.</returns>
    Task<IEnumerable<TEntity>> GetRecycleBinAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of content items in the recycle bin.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="ordering">The ordering specification. Must not be <c>null</c> — callers that don't have an opinion should use a service-layer facade that applies a default.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching recycle bin items and the total record count.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="ordering" /> is <c>null</c>.</exception>
    Task<PagedModel<TEntity>> GetPagedRecycleBinAsync(int skip, int take, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks the data integrity of content items and optionally repairs detected issues.
    /// </summary>
    /// <param name="options">Options controlling the scope and repair behaviour of the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A report describing any detected or fixed integrity issues.</returns>
    Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken);
}
