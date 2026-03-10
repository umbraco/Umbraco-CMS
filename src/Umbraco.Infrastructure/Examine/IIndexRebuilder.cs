using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Interface for rebuilding search indexes.
/// </summary>
public interface IIndexRebuilder
{
    /// <summary>
    /// Checks if the specified index can be rebuilt.
    /// </summary>
    /// <param name="indexName">The name of the index to check.</param>
    /// <returns>Whether the index can be rebuilt.</returns>
    bool CanRebuild(string indexName);

    /// <summary>
    /// Rebuilds the specified index.
    /// </summary>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <param name="delay">The delay before starting the rebuild.</param>
    /// <param name="useBackgroundThread">Whether to use a background thread for the rebuild.</param>
    [Obsolete("Use RebuildIndexesAsync() instead. Scheduled for removal in Umbraco 19.")]
    void RebuildIndex(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true);

    /// <summary>
    /// Rebuilds the specified index.
    /// </summary>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <param name="delay">The delay before starting the rebuild.</param>
    /// <param name="useBackgroundThread">Whether to use a background thread for the rebuild.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Attempt<IndexRebuildResult>> RebuildIndexAsync(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true)
    {
        RebuildIndex(indexName, delay, useBackgroundThread);
        return Task.FromResult(Attempt.Succeed(IndexRebuildResult.Success));
    }

    /// <summary>
    /// Rebuilds all indexes, or only those that are empty.
    /// </summary>
    /// <param name="onlyEmptyIndexes">Whether to only rebuild empty indexes.</param>
    /// <param name="delay">The delay before starting the rebuild.</param>
    /// <param name="useBackgroundThread">Whether to use a background thread for the rebuild.</param>
    [Obsolete("Use RebuildIndexesAsync() instead. Scheduled for removal in Umbraco 19.")]
    void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true);

    /// <summary>
    /// Rebuilds all indexes, or only those that are empty.
    /// </summary>
    /// <param name="onlyEmptyIndexes">Whether to only rebuild empty indexes.</param>
    /// <param name="delay">The delay before starting the rebuild.</param>
    /// <param name="useBackgroundThread">Whether to use a background thread for the rebuild.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Attempt<IndexRebuildResult>> RebuildIndexesAsync(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true)
    {
        RebuildIndexes(onlyEmptyIndexes, delay, useBackgroundThread);
        return Task.FromResult(Attempt.Succeed(IndexRebuildResult.Success));
    }

    /// <summary>
    /// Checks if the specified index is currently being rebuilt.
    /// </summary>
    /// <param name="indexName">The name of the index to check.</param>
    /// <returns>Whether the index is currently being rebuilt.</returns>
    // TODO (v19): Remove the default implementation.
    Task<bool> IsRebuildingAsync(string indexName) => throw new NotImplementedException();
}
