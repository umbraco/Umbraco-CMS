using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Orchestrates indexing for content changes and full index rebuilds, dispatching work to the applicable
/// <see cref="IContentChangeStrategy"/> and <see cref="IIndexer"/> for each registered index.
/// </summary>
public interface IContentIndexingService
{
    /// <summary>
    /// Queues background indexing work for the given content changes.
    /// </summary>
    /// <param name="changes">The content changes to handle.</param>
    /// <param name="origin">An identifier for the server/request that raised the changes, used for same-origin filtering.</param>
    void Handle(IEnumerable<ContentChange> changes, string origin);

    /// <summary>
    /// Queues a background rebuild of the given index.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to rebuild.</param>
    /// <param name="origin">An identifier for the server/request that requested the rebuild, used for same-origin filtering.</param>
    void Rebuild(string indexAlias, string origin);
}
