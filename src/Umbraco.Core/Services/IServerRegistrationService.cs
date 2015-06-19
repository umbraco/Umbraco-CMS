using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IServerRegistrationService
    {
        /// <summary>
        /// Touches a server to mark it as active; deactivate stale servers.
        /// </summary>
        /// <param name="serverAddress">The server url.</param>
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
        /// <returns></returns>
        IEnumerable<IServerRegistration> GetActiveServers();
    }
}