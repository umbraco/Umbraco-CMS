using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Persistence;

namespace Umbraco.Cms.Search.Core.Persistence;

/// <summary>
/// Persists <see cref="IndexDocument"/> snapshots, used for change detection so only actual field changes trigger re-indexing.
/// </summary>
public interface IIndexDocumentRepository
{
    /// <summary>
    /// Persists an index document snapshot.
    /// </summary>
    /// <param name="indexDocument">The document to persist.</param>
    public Task AddAsync(IndexDocument indexDocument);

    /// <summary>
    /// Gets a persisted index document snapshot by key.
    /// </summary>
    /// <param name="id">The document key.</param>
    /// <param name="published">Whether to look up the published or draft snapshot.</param>
    /// <returns>The persisted document, or null if none is found.</returns>
    public Task<IndexDocument?> GetAsync(Guid id, bool published);

    /// <summary>
    /// Deletes persisted index document snapshots by key.
    /// </summary>
    /// <param name="ids">The keys of the documents to delete.</param>
    /// <param name="published">Whether to delete the published or draft snapshots.</param>
    public Task DeleteAsync(Guid[] ids, bool published);

    /// <summary>
    /// Deletes all persisted index document snapshots.
    /// </summary>
    public Task DeleteAllAsync();

    /// <summary>
    /// Gets a page of persisted index document snapshots.
    /// </summary>
    /// <param name="currentPage">The zero-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>The requested page of documents.</returns>
    public Task<PagedModel<IndexDocument>> GetPagedAsync(long currentPage, int pageSize);
}
