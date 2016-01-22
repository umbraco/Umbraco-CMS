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
            ThrottleSeconds = 30; // 30 seconds
            RecurringSeconds = 60; // do it every minute
        }

        [Obsolete("This is no longer used")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ThrottleSeconds { get; set; }

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