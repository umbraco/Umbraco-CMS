namespace Umbraco.Cms.Infrastructure.Sync;

public interface ILastSyncedManager
{
    /// <summary>
    ///     Returns the last-synced id.
    /// </summary>
    int LastSyncedId { get; }

    /// <summary>
    ///     Persists the last-synced id to file.
    /// </summary>
    /// <param name="id">The id.</param>
    void SaveLastSyncedId(int id);
}
