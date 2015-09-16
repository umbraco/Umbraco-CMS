using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides server registrations to the distributed cache.
    /// </summary>
    /// <remarks>You should implement IServerRegistrar2 instead.</remarks>
    public interface IServerRegistrar
    {
        /// <summary>
        /// Gets the server registrations.
        /// </summary>
        IEnumerable<IServerAddress> Registrations { get; }
    }
}