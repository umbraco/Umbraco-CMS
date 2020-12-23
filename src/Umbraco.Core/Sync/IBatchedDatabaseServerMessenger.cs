namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> implementation that works by storing messages in the database.
    /// </summary>
    public interface IBatchedDatabaseServerMessenger : IServerMessenger
    {
        // TODO: We only ever use IBatchedDatabaseServerMessenger so just combine these interfaces

        void FlushBatch();
    }
}
