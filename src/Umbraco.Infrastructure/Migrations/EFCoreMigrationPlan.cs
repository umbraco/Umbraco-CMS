using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

public class EFCoreMigrationPlan
{
    private readonly Dictionary<string, Transition?> _transitions = new(StringComparer.InvariantCultureIgnoreCase);
    private string? _finalState;

    private string? _prevState;

    public EFCoreMigrationPlan(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(name));
        }

        Name = name;
    }

    /// <summary>
    ///     Gets the initial state.
    /// </summary>
    /// <remarks>
    ///     The initial state is the state when the plan has never
    ///     run. By default, it is the empty string, but plans may override
    ///     it if they have other ways of determining where to start from.
    /// </remarks>
    protected string InitialState { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the transitions.
    /// </summary>
    public IReadOnlyDictionary<string, Transition?> Transitions => _transitions;

    /// <summary>
    ///     Gets the name of the plan.
    /// </summary>
    public string Name { get; }

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
    ///     Sets the starting state.
    /// </summary>
    public EFCoreMigrationPlan From(string? sourceState)
    {
        _prevState = sourceState ?? throw new ArgumentNullException(nameof(sourceState));
        return this;
    }

    /// <summary>
    ///     Adds a transition to a target state through a migration.
    /// </summary>
    public EFCoreMigrationPlan To<TMigration>(string targetState)
        where TMigration : EfCoreMigrationBase
        => Add(_prevState, targetState, typeof(TMigration));

    // adds a transition
    private EFCoreMigrationPlan Add(string? sourceState, string targetState, Type? migration)
    {
        if (sourceState == null)
        {
            throw new ArgumentNullException(nameof(sourceState), $"{nameof(sourceState)} is null, {nameof(MigrationPlan)}.{nameof(From)} must not have been called.");
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

        if (!migration.Implements<EfCoreMigrationBase>())
        {
            throw new ArgumentException($"Type {migration.Name} does not implement IMigration.", nameof(migration));
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
}
