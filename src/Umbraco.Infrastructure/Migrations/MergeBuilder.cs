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
    ///     Adds a transition to a target state through an empty migration.
    /// </summary>
    public MergeBuilder To(string targetState)
        => To<NoopMigration>(targetState);

    /// <summary>
    ///     Adds a transition to a target state through a migration.
    /// </summary>
    public MergeBuilder To<TMigration>(string targetState)
        where TMigration : MigrationBase
        => To(targetState, typeof(TMigration));

    /// <summary>
    ///     Adds a transition to a target state through a migration.
    /// </summary>
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
    ///     Begins the second branch of the merge.
    /// </summary>
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
    ///     Completes the merge.
    /// </summary>
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
