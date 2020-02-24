namespace Umbraco.Core.Sync
{
    public interface IBatchedDatabaseServerMessenger : IServerMessenger
    {
        void FlushBatch();
    }
}
