namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An <see cref="IServerMessenger"/> implementation that works by storing messages in the database.
    /// </summary>
    public interface IBatchedDatabaseServerMessenger : IDatabaseServerMessenger
    {
        void FlushBatch();
        DatabaseServerMessengerOptions Options { get; }
        void Startup();
    }
}
