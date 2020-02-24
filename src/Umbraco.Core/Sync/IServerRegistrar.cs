using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides server registrations to the distributed cache.
    /// </summary>
    public interface IServerRegistrar
    {
        /// <summary>
        /// Gets the server registrations.
        /// </summary>
        IEnumerable<IServerAddress> Registrations { get; }

        /// <summary>
        /// Gets the role of the current server in the application environment.
        /// </summary>
        ServerRole GetCurrentServerRole();

    }
}
