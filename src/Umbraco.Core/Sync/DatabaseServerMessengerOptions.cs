using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        [Obsolete("This should not be used. If initialization calls need to be invoked on a cold boot, use the ISyncBootStateAccessor.Booting event.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
