using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// An interface to expose a list of server registrations for server syncing
    /// </summary>
    public interface IServerRegistrar
    {
        IEnumerable<IServerAddress> Registrations { get; } 
    }
}