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
    ///     Gets the internal integer identifiers of the versions for a content node.
    /// </summary>
    /// <param name="nodeKey">The Guid key of the content node.</param>
    /// <param name="topRows">The maximum number of version identifiers to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     Up to <paramref name="topRows" /> version identifiers, with the current version first and subsequent versions ordered most recent first.
    /// </returns>
    /// <remarks>Version rows do not yet have Guid keys.</remarks>
    // TODO: Migrate to Guid version keys when content version Guids are added to the schema.
    Task<IEnumerable<int>> GetVersionIdsAsync(Guid nodeKey, int topRows, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a specific version of a content node by its internal integer identifier.
    /// </summary>
    /// <param name="versionId">The internal integer identifier of the version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content entity at the specified version, or <c>null</c> if not found.</returns>
    /// <remarks>Version rows do not yet have Guid keys.</remarks>
    // TODO: Migrate to Guid version key when content version Guids are added to the schema.
    Task<TEntity?> GetVersionAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a specific version by its internal integer identifier.
    /// </summary>
    /// <param name="versionId">The internal integer identifier of the version to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    // TODO: Migrate to Guid version key when content version Guids are added to the schema.
    Task DeleteVersionAsync(int versionId, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes all versions of a content node that are older than the specified date.
    /// </summary>
    /// <param name="nodeKey">The Guid key of the content node.</param>
    /// <param name="versionDate">Versions with a date strictly before this value will be deleted.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteVersionsAsync(Guid nodeKey, DateTime versionDate, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the total count of content items, optionally filtered by content type alias.
    /// </summary>
    /// <param name="contentTypeAlias">
    ///     The alias of the content type to filter by, or <c>null</c> to count all content items.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of content items matching the filter.</returns>
    Task<int> CountAsync(string? contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the count of direct child content items under a given parent, optionally filtered by content type alias.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node.</param>
    /// <param name="contentTypeAlias">
    ///     The alias of the content type to filter by, or <c>null</c> to count all child content items.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of direct children matching the filter.</returns>
    Task<int> CountChildrenAsync(Guid parentKey, string? contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the count of all descendant content items under a given ancestor, optionally filtered by content type alias.
    /// </summary>
    /// <param name="parentKey">The Guid key of the ancestor node.</param>
    /// <param name="contentTypeAlias">
    ///     The alias of the content type to filter by, or <c>null</c> to count all descendants.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of descendants matching the filter.</returns>
    Task<int> CountDescendantsAsync(Guid parentKey, string? contentTypeAlias, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of direct children of a content node.
    /// </summary>
    /// <param name="parentKey">The Guid key of the parent node.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="propertyAliases">
    ///     Optional array of property aliases to load. If <c>null</c>, all properties are loaded.
    ///     If empty, no custom properties are loaded (only system properties).
    /// </param>
    /// <param name="ordering">The ordering specification, or <c>null</c> for default ordering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching children and the total record count.</returns>
    Task<PagedModel<TEntity>> GetChildrenAsync(Guid parentKey, long pageIndex, int pageSize, string[]? propertyAliases, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of all descendants of a content node.
    /// </summary>
    /// <param name="ancestorKey">The Guid key of the ancestor node.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> for default ordering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching descendants and the total record count.</returns>
    Task<PagedModel<TEntity>> GetDescendantsAsync(Guid ancestorKey, long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets all content items currently in the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All content items in the recycle bin.</returns>
    Task<IEnumerable<TEntity>> GetRecycleBinAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets a paged list of content items in the recycle bin.
    /// </summary>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="ordering">The ordering specification, or <c>null</c> for default ordering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result containing the matching recycle bin items and the total record count.</returns>
    Task<PagedModel<TEntity>> GetPagedRecycleBinAsync(long pageIndex, int pageSize, Ordering? ordering, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks the data integrity of content items and optionally repairs detected issues.
    /// </summary>
    /// <param name="options">Options controlling the scope and repair behaviour of the check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A report describing any detected or fixed integrity issues.</returns>
    Task<ContentDataIntegrityReport> CheckDataIntegrityAsync(ContentDataIntegrityReportOptions options, CancellationToken cancellationToken);
}
