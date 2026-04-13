using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class DeferredActions
{
    // the default enlist priority is 100
    // enlist with a lower priority to ensure that anything "default" runs after us
    // but greater that SafeXmlReaderWriter priority which is 60
    private const int EnlistPriority = 80;

    private readonly List<IDeferredAction> _actions = new();

    /// <summary>
    /// Retrieves the <see cref="DeferredActions"/> instance enlisted in the current scope context associated with the specified <paramref name="scopeProvider"/>.
    /// </summary>
    /// <param name="scopeProvider">The scope provider from which to obtain the current scope context.</param>
    /// <returns>
    /// The <see cref="DeferredActions"/> instance enlisted in the current scope, or <c>null</c> if no scope context is available.
    /// </returns>
    public static DeferredActions? Get(ICoreScopeProvider scopeProvider)
    {
        IScopeContext? scopeContext = scopeProvider.Context;

        return scopeContext?.Enlist(
            "examineEvents",
            () => new DeferredActions(), // creator
            (completed, actions) => // action
            {
                if (completed)
                {
                    actions?.Execute();
                }
            },
            EnlistPriority);
    }

    /// <summary>
    /// Adds the specified deferred action to the collection of actions to be executed later.
    /// </summary>
    /// <param name="action">The <see cref="IDeferredAction"/> to add to the collection.</param>
    public void Add(IDeferredAction action) => _actions.Add(action);

    private void Execute()
    {
        foreach (IDeferredAction action in _actions)
        {
            action.Execute();
        }
    }
}
