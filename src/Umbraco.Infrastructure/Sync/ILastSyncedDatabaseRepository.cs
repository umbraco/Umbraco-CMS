namespace Umbraco.Cms.Infrastructure.Sync;

internal interface ILastSyncedDatabaseRepository
{
    void SetValue(string serverKey, int id);
    int GetValue(string serverKey);
}
