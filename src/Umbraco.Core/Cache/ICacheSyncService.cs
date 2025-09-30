namespace Umbraco.Cms.Core.Cache;

public interface ICacheSyncService
{
    void SyncAll(CancellationToken cancellationToken);

    void SyncInternal(CancellationToken cancellationToken);
}
