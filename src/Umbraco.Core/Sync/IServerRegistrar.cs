using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An interface to expose a list of server registrations for server syncing
    /// </summary>
    internal interface IServerRegistrar
    {
        IEnumerable<IServerAddress> Registrations { get; } 
    }
}