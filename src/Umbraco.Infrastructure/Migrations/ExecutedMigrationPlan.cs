using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Represents the state and progress of a migration plan that has been executed.
/// </summary>
public class ExecutedMigrationPlan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.ExecutedMigrationPlan"/> class
    /// with the specified migration plan and its initial and final states.
    /// </summary>
    /// <param name="plan">The <see cref="MigrationPlan"/> that was executed.</param>
    /// <param name="initialState">The state of the migration plan before execution.</param>
    /// <param name="finalState">The state of the migration plan after execution.</param>
    public ExecutedMigrationPlan(MigrationPlan plan, string initialState, string finalState)
    {
        Plan = plan;
        InitialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
        FinalState = finalState ?? throw new ArgumentNullException(nameof(finalState));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.ExecutedMigrationPlan"/> class with the specified migration plan, states, success flag, and completed transitions.
    /// </summary>
    /// <param name="plan">The migration plan that was executed.</param>
    /// <param name="initialState">The state before the migration was executed.</param>
    /// <param name="finalState">The state after the migration was executed.</param>
    /// <param name="successful">True if the migration execution was successful; otherwise, false.</param>
    /// <param name="completedTransitions">The transitions that were completed during the migration execution.</param>
    [SetsRequiredMembers]
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
        ExecutedMigrationContexts = Array.Empty<IMigrationContext>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutedMigrationPlan"/> class, which represents a record of a completed migration plan execution.
    /// </summary>
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

    [Obsolete("Use UmbracoPlanExecutedNotification instead. Scheduled for removal in Umbraco 18.")]
    internal IReadOnlyList<IMigrationContext> ExecutedMigrationContexts { get; init; } = Array.Empty<IMigrationContext>();
}
