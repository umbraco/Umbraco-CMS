using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

internal sealed class DeferredActions
{
    // the default enlist priority is 100
    // enlist with a lower priority to ensure that anything "default" runs after us
    private const int EnlistPriority = 80;

    private readonly List<Action> _actions = new();

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
