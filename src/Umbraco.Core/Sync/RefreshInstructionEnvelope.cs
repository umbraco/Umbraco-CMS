using System.Collections.Generic;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    public class RefreshInstructionEnvelope
    {
        public RefreshInstructionEnvelope(IEnumerable<IServerAddress> servers, ICacheRefresher refresher, IEnumerable<RefreshInstruction> instructions)
        {
            Servers = servers;
            Refresher = refresher;
            Instructions = instructions;
        }

        public IEnumerable<IServerAddress> Servers { get; set; }
        public ICacheRefresher Refresher { get; set; }
        public IEnumerable<RefreshInstruction> Instructions { get; set; }
    }
}