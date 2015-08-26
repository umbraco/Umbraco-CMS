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
        /// In a multi-servers environment, the server is a slave server.
        /// </summary>
        Slave = 2,

        /// <summary>
        /// In a multi-servers environment, the server is the master server.
        /// </summary>
        Master = 3
    }
}