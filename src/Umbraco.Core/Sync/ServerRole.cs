using System;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// The role of a server in an application environment.
    /// </summary>
    public enum ServerRole : byte
    {
        /// <summary>
        /// The server role is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The server is the single server of a single-server environment.
        /// </summary>
        Single = 1,

        /// <summary>
        /// In a multi-servers environment, the server is a replica server.
        /// </summary>
        Replica = 2,

        /// <summary>
        /// In a multi-servers environment, the server is the master server.
        /// </summary>
        [Obsolete("Replaced with ServerRole.Primary. Will be removed from a future version.")]
        Master = 3,

        /// <summary>
        /// In a multi-servers environment, the server is the master server.
        /// </summary>
        Primary = 4
    }

    public static class ServerRoleExtensions
    {
        public static bool IsPrimary(this ServerRole role)
        {
            return role == ServerRole.Master
                || role == ServerRole.Primary;
        }

        public static bool ShouldPrune(this ServerRole role)
        {
            return role.IsPrimary()
                || role == ServerRole.Single;
        }
    }
}
