namespace Umbraco.Cms.Core.Sync;

public interface ILastSyncedManager
{
    Task<int?> GetLastSyncedInternalAsync();

    Task<int?> GetLastSyncedExternalAsync();

    Task SaveLastSyncedInternalAsync(int id);

    Task SaveLastSyncedExternalAsync(int id);
}
