using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
    public enum SyncBootState
    {
        /// <summary>
        /// Unknown state. Treat as HasSyncState
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Cold boot. No Sync state
        /// </summary>
        ColdBoot = 1,
        /// <summary>
        /// Warm boot. Sync state present
        /// </summary>
        HasSyncState = 2
    }
}
