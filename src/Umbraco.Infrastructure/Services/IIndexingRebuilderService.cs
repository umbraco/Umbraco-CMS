using Examine;

namespace Umbraco.Cms.Infrastructure.Services;

/// <summary>
/// Indexing rebuilder service.
/// </summary>
public interface IIndexingRebuilderService
{
    /// <summary>
    /// Checks if the index can be rebuilt.
    /// </summary>
    /// <param name="indexName">The name of the index to check.</param>
    /// <returns>Whether the index can be rebuilt.</returns>
    bool CanRebuild(string indexName);

    /// <summary>
    /// Tries to rebuild the specified index.
    /// </summary>
    /// <param name="index">The index to rebuild.</param>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <returns>Whether the rebuild was successfully scheduled.</returns>
    [Obsolete("Use TryRebuildAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool TryRebuild(IIndex index, string indexName);

    /// <summary>
    /// Tries to rebuild the specified index.
    /// </summary>
    /// <param name="index">The index to rebuild.</param>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <returns>Whether the rebuild was successfully scheduled.</returns>
    Task<bool> TryRebuildAsync(IIndex index, string indexName) => Task.FromResult(TryRebuild(index, indexName));

    /// <summary>
    /// Checks if the specified index is currently being rebuilt.
    /// </summary>
    /// <param name="indexName">The name of the index to check.</param>
    /// <returns>Whether the index is currently being rebuilt.</returns>
    [Obsolete("Use IsRebuildingAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool IsRebuilding(string indexName);

    /// <summary>
    /// Checks if the specified index is currently being rebuilt.
    /// </summary>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <returns>Whether the index is currently being rebuilt.</returns>
    Task<bool> IsRebuildingAsync(string indexName) =>
        Task.FromResult(IsRebuilding(indexName));
}
