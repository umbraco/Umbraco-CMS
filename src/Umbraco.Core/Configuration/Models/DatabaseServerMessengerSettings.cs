// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;

namespace Umbraco.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for database server messaging settings.
    /// </summary>
    public class DatabaseServerMessengerSettings
    {
        /// <summary>
        /// Gets or sets a value for the maximum number of instructions that can be processed at startup; otherwise the server cold-boots (rebuilds its caches).
        /// </summary>
        public int MaxProcessingInstructionCount { get; set; } = 1000;

        /// <summary>
        /// Gets or sets a value for the time to keep instructions in the database; records older than this number will be pruned.
        /// </summary>
        public TimeSpan TimeToRetainInstructions { get; set; } = TimeSpan.FromDays(2);

        /// <summary>
        /// Gets or sets a value for the time to wait between each sync operations.
        /// </summary>
        public TimeSpan TimeBetweenSyncOperations { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets a value for the time to wait between each prune operations.
        /// </summary>
        public TimeSpan TimeBetweenPruneOperations { get; set; } = TimeSpan.FromMinutes(1);
    }
}
