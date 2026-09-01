namespace Umbraco.Cms.Search.Provider.Examine.Services;

/// <summary>
/// Waits for a Lucene index to commit pending writes to disk, so callers can reliably check its document count
/// right after a write instead of racing the asynchronous commit.
/// </summary>
public interface IIndexCommitMonitor
{
    /// <summary>
    /// Waits for the given index to commit, up to an internal timeout.
    /// </summary>
    /// <param name="indexAlias">The physical name of the index to wait on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the index committed (or already had documents) before the timeout elapsed; otherwise false.</returns>
    Task<bool> WaitForCommitAsync(string indexAlias, CancellationToken cancellationToken);
}
