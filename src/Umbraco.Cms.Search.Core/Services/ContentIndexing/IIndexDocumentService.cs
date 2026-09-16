using Umbraco.Cms.Search.Core.Models.Persistence;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Persists and queries <see cref="IndexDocument"/> snapshots, used for change detection so only actual field
/// changes trigger re-indexing.
/// </summary>
public interface IIndexDocumentService
{
    /// <summary>
    /// Persists an index document snapshot.
    /// </summary>
    /// <param name="indexDocument">The document to persist.</param>
    Task AddAsync(IndexDocument indexDocument);

    /// <summary>
    /// Deletes the persisted document snapshots for the given content keys.
    /// </summary>
    /// <param name="ids">The content keys to delete.</param>
    /// <param name="published">Whether to delete the published or draft snapshots.</param>
    Task DeleteAsync(Guid[] ids, bool published);

    /// <summary>
    /// Gets the persisted document snapshot for a content item, if one exists.
    /// </summary>
    /// <param name="id">The content key.</param>
    /// <param name="published">Whether to get the published or draft snapshot.</param>
    /// <returns>The persisted snapshot, or null if none exists.</returns>
    Task<IndexDocument?> GetAsync(Guid id, bool published);

    /// <summary>
    /// Deletes all persisted document snapshots.
    /// </summary>
    Task DeleteAllAsync();

    /// <summary>
    /// Removes fields for the given cultures from all persisted document snapshots, e.g. after a language is deleted.
    /// </summary>
    /// <param name="isoCodes">The ISO codes of the cultures to remove.</param>
    Task DeleteCulturesAsync(IReadOnlyCollection<string> isoCodes);
}
