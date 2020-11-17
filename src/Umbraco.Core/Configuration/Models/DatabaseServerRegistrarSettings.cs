using System;

namespace Umbraco.Core.Configuration.Models
{
    public class DatabaseServerRegistrarSettings
    {
        /// <summary>
        /// The amount of time to wait between calls to the database on the background thread.
        /// </summary>
        public TimeSpan WaitTimeBetweenCalls { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// The time span to wait before considering a server stale, after it has last been accessed.
        /// </summary>
        public TimeSpan StaleServerTimeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}
