using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Services.ContentIndexing;

/// <summary>
/// Tracks content changes and translates them into index updates for the indexes that use this strategy.
/// </summary>
/// <remarks>
/// Umbraco Search ships two strategies: <see cref="IPublishedContentChangeStrategy"/> (for the published content
/// index) and <see cref="IDraftContentChangeStrategy"/> (for the draft content, media and member indexes).
/// </remarks>
public interface IContentChangeStrategy
{
    /// <summary>
    /// Applies the given content changes to the given indexes.
    /// </summary>
    /// <param name="indexInfos">The indexes using this strategy.</param>
    /// <param name="changes">The content changes to apply.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken);

    /// <summary>
    /// Rebuilds the given index from scratch, indexing all applicable content.
    /// </summary>
    /// <param name="indexInfo">The index to rebuild.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken);
}
