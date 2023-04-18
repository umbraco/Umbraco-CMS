namespace Umbraco.Cms.Infrastructure.Migrations;

public class ExecutedMigrationPlan
{
    public ExecutedMigrationPlan(MigrationPlan plan, string initialState, string finalState)
    {
        Plan = plan;
        InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
        FinalState = finalState ?? throw new ArgumentNullException(nameof(finalState));
    }

    public ExecutedMigrationPlan(
        MigrationPlan plan,
        string initialState,
        string finalState,
        bool successful,
        IReadOnlyList<MigrationPlan.Transition> completedTransitions)
    {
        Plan = plan;
        InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
        FinalState = finalState ?? throw new ArgumentNullException(nameof(finalState));
        Successful = successful;
        CompletedTransitions = completedTransitions;
    }

    public ExecutedMigrationPlan()
    {
    }

    /// <summary>
    /// The Migration plan itself.
    /// </summary>
    public required MigrationPlan Plan { get; init; }

    /// <summary>
    /// The initial state the plan started from, is null if the plan started from the beginning.
    /// </summary>
    public required string InitialState { get; init; }

    /// <summary>
    /// The final state after the migrations has ran.
    /// </summary>
    public required string FinalState { get; init; }

    /// <summary>
    /// Determines if the migration plan was a success, that is that all migrations ran successfully.
    /// </summary>
    public required bool Successful { get; init; }

    /// <summary>
    /// The exception that caused the plan to fail.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// A collection of all the succeeded transition.
    /// </summary>
    public required IReadOnlyList<MigrationPlan.Transition> CompletedTransitions { get; init; }
}
