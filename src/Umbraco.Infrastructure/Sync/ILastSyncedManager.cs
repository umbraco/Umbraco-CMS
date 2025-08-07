namespace Umbraco.Cms.Infrastructure.Sync;

public interface ILastSyncedManager
{
    Task<int?> GetLastSyncedInternalAsync();

    Task<int?> GetLastSyncedExternalAsync();

    void SaveLastSyncedInternalAsync(int id);

    void SaveLastSyncedExternalAsync(int id);
}
