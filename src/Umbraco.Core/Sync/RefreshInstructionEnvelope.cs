using System.Collections.Generic;
using umbraco.interfaces;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Used for any 'Batched' <see cref="IServerMessenger"/> instances which specifies a set of <see cref="RefreshInstruction"/> targeting a collection of 
    /// <see cref="IServerAddress"/>
    /// </summary>
    public sealed class RefreshInstructionEnvelope
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