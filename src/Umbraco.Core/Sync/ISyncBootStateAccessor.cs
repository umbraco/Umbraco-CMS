using System;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Retrieve the state of the sync service
    /// </summary>
    public interface ISyncBootStateAccessor
    {
        /// <summary>
        /// Get the boot state
        /// </summary>
        /// <returns></returns>
        SyncBootState GetSyncBootState();

        /// <summary>
        /// Raised when the boot state is known
        /// </summary>
        event EventHandler<SyncBootState> Booting;
    }
}
