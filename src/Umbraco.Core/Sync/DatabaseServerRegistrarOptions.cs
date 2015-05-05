using System;

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
            StaleServerTimeout = new TimeSpan(1,0,0); // 1 day
            ThrottleSeconds = 30; // 30 seconds
        }

        /// <summary>
        /// The number of seconds to wait between each updates to the database.
        /// </summary>
        public int ThrottleSeconds { get; set; }

        /// <summary>
        /// The time span to wait before considering a server stale, after it has last been accessed.
        /// </summary>
        public TimeSpan StaleServerTimeout { get; set; }
    }
}