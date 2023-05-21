namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Defines a result object for the <see cref="ICacheInstructionService.ProcessInstructions" />
///     operation.
/// </summary>
public class ProcessInstructionsResult
{
    private ProcessInstructionsResult()
    {
    }

    public int NumberOfInstructionsProcessed { get; private set; }

    public int LastId { get; private set; }

    public bool InstructionsWerePruned { get; private set; }

    public static ProcessInstructionsResult AsCompleted(int numberOfInstructionsProcessed, int lastId) =>
        new() { NumberOfInstructionsProcessed = numberOfInstructionsProcessed, LastId = lastId };

    public static ProcessInstructionsResult AsCompletedAndPruned(int numberOfInstructionsProcessed, int lastId) =>
        new()
        {
            NumberOfInstructionsProcessed = numberOfInstructionsProcessed,
            LastId = lastId,
            InstructionsWerePruned = true,
        };
}
