using System;
using System.ComponentModel;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides options to the <see cref="DatabaseServerRegistrar"/>.
    /// </summary>
    public sealed class DatabaseServerRegistrarOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerRegistrarOptions"/> class with default values.
        /// </summary>
        public DatabaseServerRegistrarOptions()
        {
            StaleServerTimeout = TimeSpan.FromMinutes(2); // 2 minutes
            RecurringSeconds = 60; // do it every minute
        }
        
        /// <summary>
        /// The amount of seconds to wait between calls to the database on the background thread
        /// </summary>
        public int RecurringSeconds { get; set; }

        /// <summary>
        /// The time span to wait before considering a server stale, after it has last been accessed.
        /// </summary>
        public TimeSpan StaleServerTimeout { get; set; }
    }
}
