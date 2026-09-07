using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Indexes a slice of a content item's data (e.g. system fields or property values) into search index fields.
/// </summary>
/// <remarks>
/// Implementations are registered explicitly (see <c>AddSearchCore</c>) and run in turn for every content item
/// being indexed; each contributes its own <see cref="IndexField"/> instances to the resulting document.
/// </remarks>
public interface IContentIndexer
{
    /// <summary>
    /// Builds the index fields this indexer is responsible for, for the given content item and cultures.
    /// </summary>
    /// <param name="content">The content item to index.</param>
    /// <param name="cultures">The cultures to build fields for.</param>
    /// <param name="published">Whether the published or draft version of the content is being indexed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The index fields contributed by this indexer.</returns>
    Task<IEnumerable<IndexField>> GetIndexFieldsAsync(IContentBase content, string?[] cultures, bool published, CancellationToken cancellationToken);
}
