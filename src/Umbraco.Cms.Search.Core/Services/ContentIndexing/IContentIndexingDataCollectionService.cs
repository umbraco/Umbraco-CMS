using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Gathers the full set of index fields for a content item by running it through every registered <see cref="IContentIndexer"/>.
/// </summary>
public interface IContentIndexingDataCollectionService
{
    /// <summary>
    /// Collects the index fields for a content item, reusing a persisted <see cref="Models.Persistence.IndexDocument"/> snapshot when available.
    /// </summary>
    /// <param name="content">The content item to collect fields for.</param>
    /// <param name="published">Whether to collect fields for the published or draft version of the content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The collected index fields, or null if the content has no cultures to index.</returns>
    Task<IEnumerable<IndexField>?> CollectAsync(IContentBase content, bool published, CancellationToken cancellationToken);
}
