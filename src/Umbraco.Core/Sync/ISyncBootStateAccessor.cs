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
}
