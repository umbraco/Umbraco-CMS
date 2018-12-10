using System;
using System.Collections.Generic;
using Examine;
using Umbraco.Core;

namespace Umbraco.Examine
{
    

    /// <summary>
    /// Exposes diagnostic information about an index
    /// </summary>
    public interface IIndexDiagnostics
    {
        /// <summary>
        /// The number of documents in the index
        /// </summary>
        int DocumentCount { get; }

        /// <summary>
        /// The number of fields in the index
        /// </summary>
        int FieldCount { get; }

        /// <summary>
        /// If the index can be open/read
        /// </summary>
        /// <returns>
        /// A successful attempt if it is healthy, else a failed attempt with a message if unhealthy
        /// </returns>
        Attempt<string> IsHealthy();

        /// <summary>
        /// A key/value collection of diagnostic properties for the index
        /// </summary>
        /// <remarks>
        /// Used to display in the UI
        /// </remarks>
        IReadOnlyDictionary<string, object> Metadata { get; }
    }
}
