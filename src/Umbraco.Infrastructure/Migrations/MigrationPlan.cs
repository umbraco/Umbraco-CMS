using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Represents a migration plan.
/// </summary>
public class MigrationPlan
{
    private readonly Dictionary<string, Transition?> _transitions = new(StringComparer.InvariantCultureIgnoreCase);
    private string? _finalState;

    private string? _prevState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationPlan" /> class.
    /// </summary>
    /// <param name="name">The name of the plan.</param>
    public MigrationPlan(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(name));
        }

        Name = name;
    }

    /// <summary>
    ///     If set to true the plan executor will ignore any current state persisted and
    ///     run the plan from its initial state to its end state.
    /// </summary>
    public virtual bool IgnoreCurrentState { get; } = false;

    /// <summary>
    ///     Gets the transitions.
    /// </summary>
    public IReadOnlyDictionary<string, Transition?> Transitions => _transitions;

    /// <summary>
    ///     Gets the name of the plan.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the initial state.
    /// </summary>
    /// <remarks>
    ///     The initial state is the state when the plan has never
    ///     run. By default, it is the empty string, but plans may override
    ///     it if they have other ways of determining where to start from.
    /// </remarks>
    public virtual string InitialState => string.Empty;

    /// <summary>
    ///     Gets the final state.
    /// </summary>
    public string FinalState
    {
        get
        {
            // modifying the plan clears _finalState
            // Validate() either sets _finalState, or throws
            if (_finalState == null)
            {
                Validate();
            }

            return _finalState!;
        }
    }

    /// <summary>
    ///     Adds a transition to the specified target state using an empty (no-operation) migration.
    /// </summary>
    /// <param name="targetState">The target state to transition to.</param>
    /// <returns>The updated <see cref="MigrationPlan"/> instance.</returns>
    public MigrationPlan To(string targetState)
        => To<NoopMigration>(targetState);

    // adds a transition
    private MigrationPlan Add(string? sourceState, string targetState, Type? migration)
    {
        if (sourceState == null)
        {
            throw new ArgumentNullException(
                nameof(sourceState),
                $"{nameof(sourceState)} is null, {nameof(MigrationPlan)}.{nameof(From)} must not have been called.");
        }

        if (targetState == null)
        {
            throw new ArgumentNullException(nameof(targetState));
        }

        if (string.IsNullOrWhiteSpace(targetState))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(targetState));
        }

        if (sourceState == targetState)
        {
            throw new ArgumentException("Source and target state cannot be identical.");
        }

        if (migration == null)
        {
            throw new ArgumentNullException(nameof(migration));
        }

        if (!migration.Implements<AsyncMigrationBase>())
        {
            throw new ArgumentException($"Type {migration.Name} does not implement AsyncMigrationBase.", nameof(migration));
        }

        sourceState = sourceState.Trim();
        targetState = targetState.Trim();

        // throw if we already have a transition for that state which is not null,
        // null is used to keep track of the last step of the chain
        if (_transitions.ContainsKey(sourceState) && _transitions[sourceState] != null)
        {
            throw new InvalidOperationException($"A transition from state \"{sourceState}\" has already been defined.");
        }

        // register the transition
        _transitions[sourceState] = new Transition(sourceState, targetState, migration);

        // register the target state if we don't know it already
        // this is how we keep track of the final state - because
        // transitions could be defined in any order, that might
        // be overridden afterwards.
        if (!_transitions.ContainsKey(targetState))
        {
            _transitions.Add(targetState, null);
        }

        _prevState = targetState;
        _finalState = null; // force re-validation

        return this;
    }

    /// <summary>
    /// Adds a transition to the specified target state through an empty migration.
    /// </summary>
    /// <param name="targetState">The target state to transition to.</param>
    /// <returns>The updated <see cref="MigrationPlan"/> instance.</returns>
    public MigrationPlan To(Guid targetState)
        => To<NoopMigration>(targetState.ToString());

    /// <summary>
    /// Adds a transition from the current state to the specified target state using the specified migration type.
    /// </summary>
    /// <typeparam name="TMigration">The type of <see cref="IMigration"/> to execute for the transition.</typeparam>
    /// <param name="targetState">The name of the target state to transition to.</param>
    /// <returns>The current <see cref="MigrationPlan"/> instance, allowing for method chaining.</returns>
    public MigrationPlan To<TMigration>(string targetState)
        where TMigration : AsyncMigrationBase
        => To(targetState, typeof(TMigration));

    /// <summary>
    /// Adds a transition to the specified target state using the given migration type <typeparamref name="TMigration"/>.
    /// </summary>
    /// <typeparam name="TMigration">The type of migration to apply for this transition.</typeparam>
    /// <param name="targetState">The unique identifier of the target state.</param>
    /// <returns>The updated <see cref="MigrationPlan"/> instance.</returns
    public MigrationPlan To<TMigration>(Guid targetState)
        where TMigration : AsyncMigrationBase
        => To(targetState, typeof(TMigration));

    /// <summary>
    ///     Adds a transition to a target state through a migration.
    /// </summary>
    /// <param name="targetState">The target state to transition to.</param>
    /// <param name="migration">The type of migration to apply during the transition. This should be a <see cref="Type"/> derived from <c>MigrationBase</c>, or <c>null</c> for no migration.</param>
    /// <returns>The updated <see cref="MigrationPlan"/> instance.</returns>
    public MigrationPlan To(string targetState, Type? migration)
        => Add(_prevState, targetState, migration);

    /// <summary>
    /// Adds a transition to a target state through the specified migration type.
    /// </summary>
    /// <param name="targetState">The target state to transition to.</param>
    /// <param name="migration">The type of migration to apply for the transition.</param>
    /// <returns>The updated <see cref="MigrationPlan"/> instance.</returns>
    public MigrationPlan To(Guid targetState, Type migration)
        => Add(_prevState, targetState.ToString(), migration);

    /// <summary>
    ///     Specifies the starting state for the migration plan, which determines where migrations begin execution.
    /// </summary>
    /// <param name="sourceState">The starting state to set for the migration plan.</param>
    /// <returns>The current <see cref="MigrationPlan"/> instance, allowing for method chaining.</returns>
    public MigrationPlan From(string? sourceState)
    {
        _prevState = sourceState ?? throw new ArgumentNullException(nameof(sourceState));
        return this;
    }

    /// <summary>
    ///     Adds a transition to a new target state through a migration, replacing a previous migration with a recovery migration for the old state.
    /// </summary>
    /// <typeparam name="TMigrationNew">The migration to apply for the new target state.</typeparam>
    /// <typeparam name="TMigrationRecover">The migration to use to recover from the previous target state.</typeparam>
    /// <param name="recoverState">
    ///     The previous target state that should be recovered from using
    ///     <typeparamref name="TMigrationRecover" /> before transitioning to the new target state.
    /// </param>
    /// <param name="targetState">The new target state to transition to.</param>
    /// <returns>The current <see cref="MigrationPlan" /> instance, allowing for method chaining.</returns>
    public MigrationPlan ToWithReplace<TMigrationNew, TMigrationRecover>(string recoverState, string targetState)
        where TMigrationNew : AsyncMigrationBase
        where TMigrationRecover : AsyncMigrationBase
    {
        To<TMigrationNew>(targetState);
        From(recoverState).To<TMigrationRecover>(targetState);
        return this;
    }

    /// <summary>
    ///     Adds a transition to a new target state using the specified migration, replacing a previous migration for the given state.
    /// </summary>
    /// <typeparam name="TMigrationNew">The migration to apply to reach the new target state.</typeparam>
    /// <param name="recoverState">The previous target state that can be recovered from directly.</param>
    /// <param name="targetState">The new target state to transition to.</param>
    /// <returns>The <see cref="MigrationPlan"/> instance with the updated transition.</returns>
    public MigrationPlan ToWithReplace<TMigrationNew>(string recoverState, string targetState)
        where TMigrationNew : AsyncMigrationBase
    {
        To<TMigrationNew>(targetState);
        From(recoverState).To(targetState);
        return this;
    }

    /// <summary>
    ///     Adds transitions to a <paramref name="targetState"/> by cloning the transition chain from <paramref name="startState"/> up to (but not including) <paramref name="endState"/>.
    /// </summary>
    /// <param name="startState">The initial state from which the transition chain will be cloned.</param>
    /// <param name="endState">The state at which to stop cloning transitions (exclusive).</param>
    /// <param name="targetState">The state to which the final cloned transition will point instead of <paramref name="endState"/>.</param>
    /// <returns>The current <see cref="MigrationPlan"/> instance with the added transitions.</returns>
    public MigrationPlan ToWithClone(string startState, string endState, string targetState)
    {
        if (startState == null)
        {
            throw new ArgumentNullException(nameof(startState));
        }

        if (string.IsNullOrWhiteSpace(startState))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(startState));
        }

        if (endState == null)
        {
            throw new ArgumentNullException(nameof(endState));
        }

        if (string.IsNullOrWhiteSpace(endState))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(endState));
        }

        if (targetState == null)
        {
            throw new ArgumentNullException(nameof(targetState));
        }

        if (string.IsNullOrWhiteSpace(targetState))
        {
            throw new ArgumentException(
                "Value can't be empty or consist only of white-space characters.",
                nameof(targetState));
        }

        if (startState == endState)
        {
            throw new ArgumentException("Start and end states cannot be identical.");
        }

        startState = startState.Trim();
        endState = endState.Trim();
        targetState = targetState.Trim();

        var state = startState;
        var visited = new HashSet<string>();

        while (state != endState)
        {
            if (state is null || visited.Contains(state))
            {
                throw new InvalidOperationException("A loop was detected in the copied chain.");
            }

            visited.Add(state);

            if (!_transitions.TryGetValue(state, out Transition? transition))
            {
                throw new InvalidOperationException($"There is no transition from state \"{state}\".");
            }

            var newTargetState = transition?.TargetState == endState
                ? targetState
                : CreateRandomState();
            To(newTargetState, transition?.MigrationType);
            state = transition?.TargetState;
        }

        return this;
    }

    /// <summary>
    ///     Creates a random, unique state as a string in GUID format.
    /// </summary>
    /// <returns>A string containing a randomly generated, unique GUID in braces and uppercase.</returns>
    public virtual string CreateRandomState()
        => Guid.NewGuid().ToString("B").ToUpper();

    /// <summary>
    ///     Initiates the process of merging multiple migration branches into a single branch within the migration plan.
    /// </summary>
    /// <returns>A <see cref="Umbraco.Cms.Infrastructure.Migrations.MigrationPlan.MergeBuilder" /> instance used to configure and define the merge operation.</returns>
    public MergeBuilder Merge() => new(this);

    /// <summary>
    ///     Validates the plan.
    /// </summary>
    /// <returns>The plan's final state.</returns>
    public void Validate()
    {
        if (_finalState != null)
        {
            return;
        }

        // quick check for dead ends - a dead end is a transition that has a target state
        // that is not null and does not match any source state. such a target state has
        // been registered as a source state with a null transition. so there should be only
        // one.
        string? finalState = null;
        foreach (KeyValuePair<string, Transition?> kvp in _transitions.Where(x => x.Value == null))
        {
            if (finalState == null)
            {
                finalState = kvp.Key;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Multiple final states have been detected in the plan (\"{finalState}\", \"{kvp.Key}\")."
                    + " Make sure the plan contains only one final state.");
            }
        }

        // now check for loops
        var verified = new List<string>();
        foreach (Transition? transition in _transitions.Values)
        {
            if (transition == null || verified.Contains(transition.SourceState))
            {
                continue;
            }

            var visited = new List<string> { transition.SourceState };
            Transition? nextTransition = _transitions[transition.TargetState];
            while (nextTransition != null && !verified.Contains(nextTransition.SourceState))
            {
                if (visited.Contains(nextTransition.SourceState))
                {
                    throw new InvalidOperationException(
                        $"A loop has been detected in the plan around state \"{nextTransition.SourceState}\"."
                        + " Make sure the plan does not contain circular transition paths.");
                }

                visited.Add(nextTransition.SourceState);
                nextTransition = _transitions[nextTransition.TargetState];
            }

            verified.AddRange(visited);
        }

        _finalState = finalState!;
    }

    /// <summary>
    ///     Throws an exception when the specified initial state is unknown to the migration plan.
    /// </summary>
    /// <param name="state">The initial state value that is not recognized by the migration plan.</param>
    public virtual void ThrowOnUnknownInitialState(string state) =>
        throw new InvalidOperationException($"The migration plan \"{Name}\" does not support migrating from state \"{state}\".");

    /// <summary>
    ///     Follows a path (for tests and debugging).
    /// </summary>
    /// <remarks>Does the same thing Execute does, but does not actually execute migrations.</remarks>
    internal IReadOnlyList<string> FollowPath(string? fromState = null, string? toState = null)
    {
        toState = toState?.NullOrWhiteSpaceAsNull();

        Validate();

        var origState = fromState ?? string.Empty;
        var states = new List<string> { origState };

        if (!_transitions.TryGetValue(origState, out Transition? transition))
        {
            throw new InvalidOperationException($"Unknown state \"{origState}\".");
        }

        while (transition != null)
        {
            var nextState = transition.TargetState;
            origState = nextState;
            states.Add(origState);

            if (nextState == toState)
            {
                transition = null;
                continue;
            }

            if (!_transitions.TryGetValue(origState, out transition))
            {
                throw new InvalidOperationException($"Unknown state \"{origState}\".");
            }
        }

        // safety check
        if (origState != (toState ?? _finalState))
        {
            throw new InvalidOperationException(
                $"Internal error, reached state {origState} which is not state {toState ?? _finalState}");
        }

        return states;
    }

    /// <summary>
    ///     Represents a plan transition.
    /// </summary>
    public class Transition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Transition" /> class.
        /// </summary>
        /// <param name="sourceState">The source state of the transition.</param>
        /// <param name="targetState">The target state of the transition.</param>
        /// <param name="migrationTtype">The type of the migration associated with this transition.</param>
        public Transition(string sourceState, string targetState, Type migrationTtype)
        {
            SourceState = sourceState;
            TargetState = targetState;
            MigrationType = migrationTtype;
        }

        /// <summary>
        ///     Gets the source state.
        /// </summary>
        public string SourceState { get; }

        /// <summary>
        ///     Gets the target state.
        /// </summary>
        public string TargetState { get; }

        /// <summary>
        ///     Gets the migration type.
        /// </summary>
        public Type MigrationType { get; }

        /// <inheritdoc />
        public override string ToString() =>
            MigrationType == typeof(NoopMigration)
                ? $"{(SourceState == string.Empty ? "<empty>" : SourceState)} --> {TargetState}"
                : $"{SourceState} -- ({MigrationType.FullName}) --> {TargetState}";
    }
}
