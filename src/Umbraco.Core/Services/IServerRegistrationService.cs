using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Sync;

namespace Umbraco.Core.Services
{
    public interface IServerRegistrationService
    {
        /// <summary>
        /// Touches a server to mark it as active; deactivate stale servers.
        /// </summary>
        /// <param name="serverAddress">The server URL.</param>
        /// <param name="serverIdentity">The server unique identity.</param>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        void TouchServer(string serverAddress, string serverIdentity, TimeSpan staleTimeout);

        /// <summary>
        /// Deactivates a server.
        /// </summary>
        /// <param name="serverIdentity">The server unique identity.</param>
        void DeactiveServer(string serverIdentity);

        /// <summary>
        /// Deactivates stale servers.
        /// </summary>
        /// <param name="staleTimeout">The time after which a server is considered stale.</param>
        void DeactiveStaleServers(TimeSpan staleTimeout);

        /// <summary>
        /// Return all active servers.
        /// </summary>
        /// <param name="refresh">A value indicating whether to force-refresh the cache.</param>
        /// <returns>All active servers.</returns>
        /// <remarks>By default this method will rely on the repository's cache, which is updated each
        /// time the current server is touched, and the period depends on the configuration. Use the
        /// <paramref name="refresh"/> parameter to force a cache refresh and reload active servers
        /// from the database.</remarks>
        IEnumerable<IServerRegistration> GetActiveServers(bool refresh = false);

        /// <summary>
        /// Gets the current server identity.
        /// </summary>
        string CurrentServerIdentity { get; }

        /// <summary>
        /// Gets the role of the current server.
        /// </summary>
        /// <returns>The role of the current server.</returns>
        ServerRole GetCurrentServerRole();
    }
}
