using System;
using System.Collections.Generic;

namespace Umbraco.Cms.Core.Sync
{
    /// <summary>
    /// Holds a list of callbacks associated with implementations of <see cref="IServerMessenger"/>.
    /// </summary>
    public class DatabaseServerMessengerCallbacks
    {
        /// <summary>
        /// A list of callbacks that will be invoked if the lastsynced.txt file does not exist.
        /// </summary>
        /// <remarks>
        /// These callbacks will typically be for e.g. rebuilding the xml cache file, or examine indexes, based on
        /// the data in the database to get this particular server node up to date.
        /// </remarks>
        public IEnumerable<Action> InitializingCallbacks { get; set; }
    }
}
