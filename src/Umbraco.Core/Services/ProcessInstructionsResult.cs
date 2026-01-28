namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines a result object for the <see cref="ICacheInstructionService.ProcessInstructions" />
///     operation.
/// </summary>
public class ProcessInstructionsResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProcessInstructionsResult" /> class.
    /// </summary>
    private ProcessInstructionsResult()
    {
    }

    /// <summary>
    ///     Gets the number of instructions that were processed.
    /// </summary>
    public int NumberOfInstructionsProcessed { get; private set; }

    /// <summary>
    ///     Gets the ID of the last instruction that was processed.
    /// </summary>
    public int LastId { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the instructions were pruned.
    /// </summary>
    [Obsolete("Instruction pruning has been moved to a separate background job. Scheduled for removal in V18.")]
    public bool InstructionsWerePruned { get; private set; }

    /// <summary>
    ///     Creates a new completed result with the specified number of instructions processed and last ID.
    /// </summary>
    /// <param name="numberOfInstructionsProcessed">The number of instructions that were processed.</param>
    /// <param name="lastId">The ID of the last instruction that was processed.</param>
    /// <returns>A new <see cref="ProcessInstructionsResult" /> instance.</returns>
    public static ProcessInstructionsResult AsCompleted(int numberOfInstructionsProcessed, int lastId) =>
        new() { NumberOfInstructionsProcessed = numberOfInstructionsProcessed, LastId = lastId };

    /// <summary>
    ///     Creates a new completed result with the specified number of instructions processed, last ID, and pruned flag set to true.
    /// </summary>
    /// <param name="numberOfInstructionsProcessed">The number of instructions that were processed.</param>
    /// <param name="lastId">The ID of the last instruction that was processed.</param>
    /// <returns>A new <see cref="ProcessInstructionsResult" /> instance with the pruned flag set.</returns>
    [Obsolete("Instruction pruning has been moved to a separate background job. Scheduled for removal in V18.")]
    public static ProcessInstructionsResult AsCompletedAndPruned(int numberOfInstructionsProcessed, int lastId) =>
        new()
        {
            NumberOfInstructionsProcessed = numberOfInstructionsProcessed,
            LastId = lastId,
            InstructionsWerePruned = true,
        };
}
