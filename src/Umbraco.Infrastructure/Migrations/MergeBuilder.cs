namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Represents a migration plan builder for merges.
/// </summary>
public class MergeBuilder
{
    private readonly List<Type> _migrations = new();
    private readonly MigrationPlan _plan;
    private bool _with;
    private string? _withLast;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MergeBuilder" /> class.
    /// </summary>
    internal MergeBuilder(MigrationPlan plan) => _plan = plan;

    /// <summary>
    ///     Adds a transition from the current state to the specified target state using an empty (no-operation) migration.
    /// </summary>
    /// <param name="targetState">The target state to transition to.</param>
    /// <returns>The current <see cref="Umbraco.Cms.Infrastructure.Migrations.MergeBuilder"/> instance.</returns>
    public MergeBuilder To(string targetState)
        => To<NoopMigration>(targetState);

    /// <summary>
    /// Adds a transition to the specified target state using the given migration type.
    /// </summary>
    /// <typeparam name="TMigration">The type of migration to apply during the transition.</typeparam>
    /// <param name="targetState">The target state to transition to.</param>
    /// <returns>The current <see cref="MergeBuilder"/> instance for chaining.</returns>
    public MergeBuilder To<TMigration>(string targetState)
        where TMigration : AsyncMigrationBase
        => To(targetState, typeof(TMigration));

    /// <summary>
    ///     Adds a transition from the current state to the specified target state using the provided migration type.
    /// </summary>
    /// <param name="targetState">The name of the target state to transition to.</param>
    /// <param name="migration">The <see cref="Type"/> of the migration that performs the transition.</param>
    /// <returns>The current <see cref="Umbraco.Cms.Infrastructure.Migrations.MergeBuilder"/> instance for method chaining.</returns>
    public MergeBuilder To(string targetState, Type migration)
    {
        if (_with)
        {
            _withLast = targetState;
            targetState = _plan.CreateRandomState();
        }
        else
        {
            _migrations.Add(migration);
        }

        _plan.To(targetState, migration);
        return this;
    }

    /// <summary>
    ///     Marks the start of the second branch in the merge operation, enabling further configuration.
    /// </summary>
    /// <returns>The current <see cref="Umbraco.Cms.Infrastructure.Migrations.MergeBuilder"/> instance for method chaining.</returns>
    public MergeBuilder With()
    {
        if (_with)
        {
            throw new InvalidOperationException("Cannot invoke With() twice.");
        }

        _with = true;
        return this;
    }

    /// <summary>
    ///     Finalizes the merge operation by updating the migration plan to reach the specified target state.
    /// </summary>
    /// <param name="targetState">The final state to which the migration plan should merge.</param>
    /// <returns>The <see cref="MigrationPlan"/> representing the completed merge.</returns>
    public MigrationPlan As(string targetState)
    {
        if (!_with)
        {
            throw new InvalidOperationException("Cannot invoke As() without invoking With() first.");
        }

        // reach final state
        _plan.To(targetState);

        // restart at former end of branch2
        _plan.From(_withLast);

        // and replay all branch1 migrations
        foreach (Type migration in _migrations)
        {
            _plan.To(_plan.CreateRandomState(), migration);
        }

        // reaching final state
        _plan.To(targetState);

        return _plan;
    }
}
