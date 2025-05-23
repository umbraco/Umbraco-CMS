using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Examine;

internal class DeferredActions
{
    // the default enlist priority is 100
    // enlist with a lower priority to ensure that anything "default" runs after us
    // but greater that SafeXmlReaderWriter priority which is 60
    private const int EnlistPriority = 80;

    private readonly List<IDeferredAction> _actions = new();

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

    public void Add(IDeferredAction action) => _actions.Add(action);

    private void Execute()
    {
        foreach (IDeferredAction action in _actions)
        {
            action.Execute();
        }
    }
}
