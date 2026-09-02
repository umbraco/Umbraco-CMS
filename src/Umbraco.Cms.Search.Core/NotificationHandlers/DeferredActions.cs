using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Collects actions to run once the ambient Umbraco scope completes, so indexing work runs after the triggering transaction commits.
/// </summary>
internal sealed class DeferredActions
{
    // the default enlist priority is 100
    // enlist with a lower priority to ensure that anything "default" runs after us
    private const int EnlistPriority = 80;

    private readonly List<Action> _actions = new();

    /// <summary>
    /// Gets the <see cref="DeferredActions"/> enlisted on the current scope context, if any, creating it on first use.
    /// </summary>
    /// <param name="scopeProvider">The scope provider to enlist on.</param>
    /// <returns>The enlisted <see cref="DeferredActions"/>, or null if there is no ambient scope context.</returns>
    public static DeferredActions? Get(ICoreScopeProvider scopeProvider)
    {
        IScopeContext? scopeContext = scopeProvider.Context;

        return scopeContext?.Enlist(
            "umbDeferredIndexing",
            () => new DeferredActions(),
            (completed, deferredActions) =>
            {
                if (completed && deferredActions is not null)
                {
                    deferredActions.Execute();
                }
            },
            EnlistPriority);
    }

    /// <summary>
    /// Adds an action to run once the ambient scope completes.
    /// </summary>
    /// <param name="action">The action to run.</param>
    public void Add(Action action)
        => _actions.Add(action);

    private void Execute()
    {
        foreach (Action action in _actions)
        {
            action.Invoke();
        }
    }
}
