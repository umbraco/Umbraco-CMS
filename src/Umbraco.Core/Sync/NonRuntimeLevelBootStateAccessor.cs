using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Boot state implementation for when umbraco is not in the run state
    /// </summary>
    public class NonRuntimeLevelBootStateAccessor : ISyncBootStateAccessor
    {
        public event EventHandler<SyncBootState> Booting;

        public SyncBootState GetSyncBootState()
        {
            return SyncBootState.Unknown;
        }
    }
}
