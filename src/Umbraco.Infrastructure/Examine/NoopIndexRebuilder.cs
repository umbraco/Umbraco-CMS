namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class NoopIndexRebuilder : IIndexRebuilder
{
    /// <summary>
    /// Indicates whether the specified index can be rebuilt. Always returns <c>false</c> because rebuilding is not supported by this implementation.
    /// </summary>
    /// <param name="indexName">The name of the index to check.</param>
    /// <returns><c>false</c> in all cases.</returns>
    public bool CanRebuild(string indexName) => false;

    /// <summary>
    /// Performs no action when called; this method is a no-operation (noop) implementation for rebuilding the specified index.
    /// </summary>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <param name="delay">An optional delay before starting the rebuild.</param>
    /// <param name="useBackgroundThread">Indicates whether to use a background thread for the rebuild.</param>
    public void RebuildIndex(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true) {}

    /// <summary>
    /// Simulates rebuilding indexes but intentionally performs no operation.
    /// This method is typically used as a placeholder where index rebuilding is not required.
    /// </summary>
    /// <param name="onlyEmptyIndexes">Indicates whether to rebuild only empty indexes (ignored).</param>
    /// <param name="delay">An optional delay before starting the rebuild (ignored).</param>
    /// <param name="useBackgroundThread">Indicates whether to use a background thread for the rebuild (ignored).</param>
    public void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true) {}

    /// <summary>
    /// Asynchronously determines whether the specified index is currently rebuilding.
    /// </summary>
    /// <param name="indexName">The name of the index to check.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is always <c>false</c> in this implementation, indicating the index is never rebuilding.
    /// </returns>
    public Task<bool> IsRebuildingAsync(string indexName) => Task.FromResult(false);
}
