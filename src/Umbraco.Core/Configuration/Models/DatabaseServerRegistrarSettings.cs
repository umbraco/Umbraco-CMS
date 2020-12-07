// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for database server registrar settings.
    /// </summary>
    public class DatabaseServerRegistrarSettings
    {
        /// <summary>
        /// Gets or sets a value for the amount of time to wait between calls to the database on the background thread.
        /// </summary>
        public TimeSpan WaitTimeBetweenCalls { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Gets or sets a value for the time span to wait before considering a server stale, after it has last been accessed.
        /// </summary>
        public TimeSpan StaleServerTimeout { get; set; } = TimeSpan.FromMinutes(2);
    }
}
