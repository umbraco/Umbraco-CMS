using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Gets the current server's <see cref="ServerRole"/>
    /// </summary>
    public interface IServerRoleAccessor
    {
        /// <summary>
        /// Gets the role of the current server in the application environment.
        /// </summary>
        ServerRole CurrentServerRole { get; }
    }
}
