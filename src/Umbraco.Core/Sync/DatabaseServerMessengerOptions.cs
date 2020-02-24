﻿using System;
using System.Collections.Generic;

namespace Umbraco.Core.Sync
{
    /// <summary>
    /// Provides options to the <see cref="DatabaseServerMessenger"/>.
    /// </summary>
    public class DatabaseServerMessengerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerMessengerOptions"/> with default values.
        /// </summary>
        public DatabaseServerMessengerOptions()
        {
            DaysToRetainInstructions = 2; // 2 days
            ThrottleSeconds = 5; // 5 second
            MaxProcessingInstructionCount = 1000;
            PruneThrottleSeconds = 60; // 1 minute
        }

        /// <summary>
        /// The maximum number of instructions that can be processed at startup; otherwise the server cold-boots (rebuilds its caches).
        /// </summary>
        public int MaxProcessingInstructionCount { get; set; }

        /// <summary>
        /// A list of callbacks that will be invoked if the lastsynced.txt file does not exist.
        /// </summary>
        /// <remarks>
        /// These callbacks will typically be for eg rebuilding the xml cache file, or examine indexes, based on
        /// the data in the database to get this particular server node up to date.
        /// </remarks>
        public IEnumerable<Action> InitializingCallbacks { get; set; }

        /// <summary>
        /// The number of days to keep instructions in the database; records older than this number will be pruned.
        /// </summary>
        public int DaysToRetainInstructions { get; set; }

        /// <summary>
        /// The number of seconds to wait between each sync operations.
        /// </summary>
        public int ThrottleSeconds { get; set; }

        /// <summary>
        /// The number of seconds to wait between each prune operations.
        /// </summary>
        public int PruneThrottleSeconds { get; set; }
    }
}
