using System;

namespace Umbraco.Cms.Core.Services
{
    /// <summary>
    /// Defines a result object for the <see cref="ICacheInstructionService.ProcessInstructions(bool, DateTime)"/> operation.
    /// </summary>
    public class CacheInstructionServiceProcessInstructionsResult
    {
        private CacheInstructionServiceProcessInstructionsResult()
        {
        }

        public int NumberOfInstructionsProcessed { get; private set; }

        public int LastId { get; private set; }

        public bool InstructionsWerePruned { get; private set; }

        public static CacheInstructionServiceProcessInstructionsResult AsCompleted(int numberOfInstructionsProcessed, int lastId) =>
            new CacheInstructionServiceProcessInstructionsResult { NumberOfInstructionsProcessed = numberOfInstructionsProcessed, LastId = lastId };

        public static CacheInstructionServiceProcessInstructionsResult AsCompletedAndPruned(int numberOfInstructionsProcessed, int lastId) =>
            new CacheInstructionServiceProcessInstructionsResult { NumberOfInstructionsProcessed = numberOfInstructionsProcessed, LastId = lastId, InstructionsWerePruned = true };
    };
}
