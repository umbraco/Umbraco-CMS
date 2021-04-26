using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Sync;

namespace Umbraco.Tests.TestHelpers
{
    class TestSyncBootStateAccessor : ISyncBootStateAccessor
    {
        private readonly SyncBootState _syncBootState;

        public TestSyncBootStateAccessor(SyncBootState syncBootState)
        {
            _syncBootState = syncBootState;
        }

        public event EventHandler<SyncBootState> Booting;

        public SyncBootState GetSyncBootState()
        {
            return _syncBootState;
        }
    }
}
