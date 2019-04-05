namespace Umbraco.Core.Sync
{
    /// <summary>
    ///     Persistence of the information about the last read value from the cache instructions
    /// </summary>
    public interface IServerMessengerSyncRepository
    {
        /// <summary>
        ///     Gets the incremental value.
        /// </summary>
        int Value { get; }

        /// <summary>
        ///     Updates the in-memory last-synced id and persists it to file.
        /// </summary>
        /// <param name="value">The incremental value.</param>
        void Save(int value);

        /// <summary>
        ///     DisableElectionForSingleServer
        ///     Reads the last-synced id from file into memory.
        /// </summary>
        void Read();

        /// <summary>
        ///     Resets the value to its initial value.
        /// </summary>
        void Reset();
    }
}
