namespace Umbraco.Core.Sync
{
    /// <summary>
    /// The current status of the server in the Umbraco environment
    /// </summary>
    internal enum CurrentServerEnvironmentStatus
    {
        /// <summary>
        /// If the current server is detected as the 'master' server when configured in a load balanced scenario
        /// </summary>
        Master,

        /// <summary>
        /// If the current server is detected as a 'slave' server when configured in a load balanced scenario
        /// </summary>
        Slave,

        /// <summary>
        /// If the current server cannot be detected as a 'slave' or 'master' when configured in a load balanced scenario
        /// </summary>
        Unknown,

        /// <summary>
        /// If load balancing is not enabled and this is the only server in the umbraco environment
        /// </summary>
        Single
    }
}